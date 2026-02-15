using FluentValidation;
using MediatR;
using SpaceTruckers.Api.Contracts;
using SpaceTruckers.Api.Validation;
using SpaceTruckers.Application.Trips.Commands;
using SpaceTruckers.Application.Trips.Queries;
using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Api.Endpoints;

public static class TripEndpoints
{
    private sealed class TripEndpoint;
    public static IEndpointRouteBuilder MapTripEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/trips")
            .WithTags("Trips");

        group.MapPost("/", CreateTripAsync)
            .WithName("CreateTrip");

        group.MapGet("/{tripId:guid}", GetTripAsync)
            .WithName("GetTrip");

        group.MapGet("/{tripId:guid}/summary", GetTripSummaryAsync)
            .WithName("GetTripSummary");

        group.MapPost("/{tripId:guid}/start", StartTripAsync)
            .WithName("StartTrip");

        group.MapPost("/{tripId:guid}/checkpoints/reach", ReachCheckpointAsync)
            .WithName("ReachCheckpoint");

        group.MapPost("/{tripId:guid}/incidents", ReportIncidentAsync)
            .WithName("ReportIncident");

        group.MapPost("/{tripId:guid}/complete", CompleteTripAsync)
            .WithName("CompleteTrip");

        return endpoints;
    }

    private static async Task<IResult> CreateTripAsync(
        CreateTripRequest request,
        IValidator<CreateTripRequest> validator,
        IMediator mediator,
        ILogger<TripEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var tripGuid = request.TripId ?? Guid.CreateVersion7();
        var tripId = new TripId(tripGuid);

        using var tripIdScope = logger.BeginScope("TripId={TripId}", request.DriverId);
        using var vehicleIdScope = logger.BeginScope("VehicleId={VehicleId}", request.VehicleId);
        using var routeIdScope = logger.BeginScope("RouteId={DriverId}", request.RouteId);
        using var driverIdScope = logger.BeginScope("DriverId={DriverId}", request.DriverId);

        var validation = await ValidationExtensions.ValidateAsync(request, validator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var dto = await mediator.Send(
            new CreateTripCommand(
                tripId,
                new DriverId(request.DriverId),
                new VehicleId(request.VehicleId),
                new RouteId(request.RouteId),
                request.CargoRequirement),
            cancellationToken);

        return Results.Created($"/api/trips/{dto.TripId}", dto);
    }

    private static async Task<IResult> GetTripAsync(
        Guid tripId,
        IMediator mediator,
        ILogger<TripEndpoint> logger,
        CancellationToken cancellationToken)
    {
        using var tripIdScope = logger.BeginScope("TripId={TripId}", tripId);

        var dto = await mediator.Send(new GetTripQuery(new TripId(tripId)), cancellationToken);
        return Results.Ok(dto);
    }

    private static async Task<IResult> GetTripSummaryAsync(
        Guid tripId,
        IMediator mediator,
        ILogger<TripEndpoint> logger,
        CancellationToken cancellationToken)
    {
        using var tripIdScope = logger.BeginScope("TripId={TripId}", tripId);

        var dto = await mediator.Send(new GetTripSummaryQuery(new TripId(tripId)), cancellationToken);
        return Results.Ok(dto);
    }

    private static async Task<IResult> StartTripAsync(
        Guid tripId,
        StartTripRequest request,
        IValidator<StartTripRequest> validator,
        IMediator mediator,
        ILogger<TripEndpoint> logger,
        CancellationToken cancellationToken)
    {
        using var tripIdScope = logger.BeginScope("TripId={TripId}", tripId);
        using var requestIdScope = logger.BeginScope("RequestId={RequestId}", request.RequestId);

        var validation = await ValidationExtensions.ValidateAsync(request, validator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var dto = await mediator.Send(new StartTripCommand(new TripId(tripId), request.RequestId), cancellationToken);
        return Results.Ok(dto);
    }

    private static async Task<IResult> ReachCheckpointAsync(
        Guid tripId,
        ReachCheckpointRequest request,
        IValidator<ReachCheckpointRequest> validator,
        IMediator mediator,
        ILogger<TripEndpoint> logger,
        CancellationToken cancellationToken)
    {
        using var tripIdScope = logger.BeginScope("TripId={TripId}", tripId);
        using var checkpointNameScope = logger.BeginScope("CheckpointName={CheckpointName}", request.CheckpointName);

        var validation = await ValidationExtensions.ValidateAsync(request, validator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var dto = await mediator.Send(new ReachCheckpointCommand(new TripId(tripId), request.CheckpointName), cancellationToken);
        return Results.Ok(dto);
    }

    private static async Task<IResult> ReportIncidentAsync(
        Guid tripId,
        ReportIncidentRequest request,
        IValidator<ReportIncidentRequest> validator,
        IMediator mediator,
        ILogger<TripEndpoint> logger,
        CancellationToken cancellationToken)
    {
        using var tripIdScope = logger.BeginScope("TripId={TripId}", tripId);

        var validation = await ValidationExtensions.ValidateAsync(request, validator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var dto = await mediator.Send(
            new ReportIncidentCommand(new TripId(tripId), request.Type, request.Severity, request.Description),
            cancellationToken);

        return Results.Ok(dto);
    }

    private static async Task<IResult> CompleteTripAsync(
        Guid tripId,
        IMediator mediator,
        ILogger<TripEndpoint> logger,
        CancellationToken cancellationToken)
    {
        using var tripIdScope = logger.BeginScope("TripId={TripId}", tripId);

        var dto = await mediator.Send(new CompleteTripCommand(new TripId(tripId)), cancellationToken);
        return Results.Ok(dto);
    }
}
