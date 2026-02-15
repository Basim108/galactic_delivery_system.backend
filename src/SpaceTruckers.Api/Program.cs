using FluentValidation;
using Microsoft.FeatureManagement;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SpaceTruckers.Api.Common;
using SpaceTruckers.Api.Endpoints;
using SpaceTruckers.Api.Middleware;
using SpaceTruckers.Application;
using SpaceTruckers.Application.Observability;
using SpaceTruckers.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Structured logging (built-in console logger). Formatter is selected via appsettings.{Environment}.json.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// OpenTelemetry (metrics + tracing).
builder.Services.AddOpenTelemetry()
       .ConfigureResource(r => r.AddService(serviceName: builder.Environment.ApplicationName))
       .WithMetrics(metrics => metrics
                               .AddMeter(ApiMetrics.METER_NAME)
                               .AddMeter(SpaceTruckersMetrics.METER_NAME)
                               .AddAspNetCoreInstrumentation()
                               .AddHttpClientInstrumentation()
                               .AddRuntimeInstrumentation()
                               // Explicit buckets enable percentile queries (p50/p95/p99) in the metrics backend.
                               .AddView(
                                        instrumentName: "http_endpoint_duration_ms",
                                        new ExplicitBucketHistogramConfiguration
                                        {
                                            Boundaries = [1, 2.5, 5, 10, 25, 50, 100, 250, 500, 1000, 2500, 5000, 10000,],
                                        })
                               .AddOtlpExporter())
       .WithTracing(tracing => tracing
                               .AddAspNetCoreInstrumentation()
                               .AddHttpClientInstrumentation()
                               .AddOtlpExporter());

// Feature flags:
// - Development: from appsettings.Development.json
// - Staging/Production: from Azure App Configuration
if (!builder.Environment.IsDevelopment())
{
    var appConfigConnectionString = builder.Configuration["AzureAppConfiguration:ConnectionString"];
    if (string.IsNullOrWhiteSpace(appConfigConnectionString))
    {
        throw new InvalidOperationException(
            "Azure App Configuration connection string is not configured. Set AzureAppConfiguration:ConnectionString.");
    }

    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        options
            .Connect(appConfigConnectionString)
            .UseFeatureFlags();
    });

    builder.Services.AddAzureAppConfiguration();
}

builder.Services.AddFeatureManagement();

builder.Services.AddOpenApi();

builder.Services.AddSpaceTruckersApplication();
builder.Services.AddSpaceTruckersInfrastructure(builder.Configuration);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Metrics should wrap the whole pipeline, including exception handling.
app.UseMiddleware<EndpointTimingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseAzureAppConfiguration();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
else
{
    app.MapOpenApi();
}

app.MapDriverEndpoints();
app.MapVehicleEndpoints();
app.MapRouteEndpoints();
app.MapTripEndpoints();

app.Run();

public partial class Program;
