using MediatR;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Application.Exceptions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.Application.Trips.Commands;

public sealed record CompleteTripCommand(TripId TripId) : IRequest<TripSummaryDto>;

public sealed class CompleteTripHandler(
    ITripRepository tripRepository,
    IResourceBookingService resourceBookingService,
    IClock clock,
    IDomainEventPublisher domainEventPublisher)
    : IRequestHandler<CompleteTripCommand, TripSummaryDto>
{
    public async Task<TripSummaryDto> Handle(CompleteTripCommand request, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(request.TripId, cancellationToken)
            ?? throw new NotFoundException($"Trip '{request.TripId}' was not found.");

        var expectedVersion = trip.Version;

        trip.Complete(clock.UtcNow);

        await tripRepository.UpdateAsync(trip, expectedVersion, cancellationToken);
        await resourceBookingService.ReleaseAsync(trip.DriverId, trip.VehicleId, trip.Id, cancellationToken);
        await domainEventPublisher.PublishAsync(trip.DequeueUncommittedEvents(), cancellationToken);

        return TripSummaryMapper.From(trip);
    }
}
