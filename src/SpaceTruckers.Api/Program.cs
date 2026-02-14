using FluentValidation;
using Microsoft.FeatureManagement;
using SpaceTruckers.Api.Endpoints;
using SpaceTruckers.Api.Middleware;
using SpaceTruckers.Application;
using SpaceTruckers.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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
