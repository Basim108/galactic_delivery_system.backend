using MediatR;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Application.Exceptions;
using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Application.Trips.Commands;

public sealed record ReachCheckpointCommand(TripId TripId, string CheckpointName) : IRequest<TripDto>;

public sealed class ReachCheckpointHandler(
    ITripRepository tripRepository,
    IClock clock,
    IDomainEventPublisher domainEventPublisher)
    : IRequestHandler<ReachCheckpointCommand, TripDto>
{
    public async Task<TripDto> Handle(ReachCheckpointCommand request, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(request.TripId, cancellationToken)
            ?? throw new NotFoundException($"Trip '{request.TripId}' was not found.");

        var expectedVersion = trip.Version;

        trip.ReachCheckpoint(request.CheckpointName, clock.UtcNow);

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
