using FluentValidation;
using SpaceTruckers.Api.Endpoints;
using SpaceTruckers.Api.Middleware;
using SpaceTruckers.Application;
using SpaceTruckers.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddSpaceTruckersApplication();
builder.Services.AddSpaceTruckersInfrastructure();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

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
