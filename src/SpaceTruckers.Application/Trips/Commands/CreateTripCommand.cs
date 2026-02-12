using MediatR;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Application.Exceptions;
using SpaceTruckers.Domain.Common;
using SpaceTruckers.Domain.Exceptions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Resources;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.Application.Trips.Commands;

public sealed record CreateTripCommand(
    TripId TripId,
    DriverId DriverId,
    VehicleId VehicleId,
    RouteId RouteId,
    int CargoRequirement) : IRequest<TripDto>;

public sealed class CreateTripHandler(
    ITripRepository tripRepository,
    IDriverRepository driverRepository,
    IVehicleRepository vehicleRepository,
    IRouteRepository routeRepository,
    IResourceBookingService resourceBookingService,
    IClock clock,
    IDomainEventPublisher domainEventPublisher)
    : IRequestHandler<CreateTripCommand, TripDto>
{
    public async Task<TripDto> Handle(CreateTripCommand request, CancellationToken cancellationToken)
    {
        var driver = await driverRepository.GetAsync(request.DriverId, cancellationToken)
            ?? throw new NotFoundException($"Driver '{request.DriverId}' was not found.");

        if (driver.Status != ResourceStatus.Available)
        {
            throw new DomainRuleViolationException(DomainErrorCodes.DRIVER_UNAVAILABLE, "Driver is unavailable.");
        }

        var vehicle = await vehicleRepository.GetAsync(request.VehicleId, cancellationToken)
            ?? throw new NotFoundException($"Vehicle '{request.VehicleId}' was not found.");

        if (vehicle.Status != ResourceStatus.Available)
        {
            throw new DomainRuleViolationException(DomainErrorCodes.VEHICLE_UNAVAILABLE, "Vehicle is unavailable.");
        }

        var route = await routeRepository.GetAsync(request.RouteId, cancellationToken)
            ?? throw new NotFoundException($"Route '{request.RouteId}' was not found.");

        var tripCheckpoints = route.Checkpoints
            .OrderBy(c => c.Sequence)
            .Select(c => new TripCheckpoint(c.Id, c.Sequence, c.Name))
            .ToArray();

        var trip = Trip.Create(
            request.TripId,
            request.DriverId,
            request.VehicleId,
            request.RouteId,
            CargoRequirement.From(request.CargoRequirement),
            tripCheckpoints,
            clock.UtcNow);

        // Reserve shared resources before persisting the Trip.
        await resourceBookingService.ReserveAsync(request.DriverId, request.VehicleId, request.TripId, cancellationToken);

        try
        {
            await tripRepository.AddAsync(trip, cancellationToken);
        }
        catch
        {
            await resourceBookingService.ReleaseAsync(request.DriverId, request.VehicleId, request.TripId, cancellationToken);
            throw;
        }

        await domainEventPublisher.PublishAsync(trip.DequeueUncommittedEvents(), cancellationToken);

        return new TripDto(
            trip.Id.Value,
            trip.DriverId.Value,
            trip.VehicleId.Value,
            trip.RouteId.Value,
            trip.Status,
            trip.Version,
            trip.LastReachedCheckpointName);
    }
}
