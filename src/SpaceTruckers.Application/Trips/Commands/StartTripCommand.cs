using MediatR;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Application.Exceptions;
using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Application.Trips.Commands;

public sealed record StartTripCommand(TripId TripId, string? RequestId) : IRequest<TripDto>;

public sealed class StartTripHandler(
    ITripRepository tripRepository,
    IVehicleRepository vehicleRepository,
    IClock clock,
    IDomainEventPublisher domainEventPublisher)
    : IRequestHandler<StartTripCommand, TripDto>
{
    public async Task<TripDto> Handle(StartTripCommand request, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(request.TripId, cancellationToken)
            ?? throw new NotFoundException($"Trip '{request.TripId}' was not found.");

        var expectedVersion = trip.Version;

        var vehicle = await vehicleRepository.GetAsync(trip.VehicleId, cancellationToken)
            ?? throw new NotFoundException($"Vehicle '{trip.VehicleId}' was not found.");

        trip.Start(vehicle.CargoCapacity, clock.UtcNow, request.RequestId);

        await tripRepository.UpdateAsync(trip, expectedVersion, cancellationToken);
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
