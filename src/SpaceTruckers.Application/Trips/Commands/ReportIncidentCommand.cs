using MediatR;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Application.Exceptions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.Application.Trips.Commands;

public sealed record ReportIncidentCommand(
    TripId TripId,
    string Type,
    IncidentSeverity Severity,
    string? Description) : IRequest<TripDto>;

public sealed class ReportIncidentHandler(
    ITripRepository tripRepository,
    IResourceBookingService resourceBookingService,
    IClock clock,
    IDomainEventPublisher domainEventPublisher)
    : IRequestHandler<ReportIncidentCommand, TripDto>
{
    public async Task<TripDto> Handle(ReportIncidentCommand request, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(request.TripId, cancellationToken)
            ?? throw new NotFoundException($"Trip '{request.TripId}' was not found.");

        var expectedVersion = trip.Version;

        trip.ReportIncident(new Incident(request.Type, request.Severity, request.Description), clock.UtcNow);

        await tripRepository.UpdateAsync(trip, expectedVersion, cancellationToken);

        if (trip.Status == TripStatus.Aborted)
        {
            await resourceBookingService.ReleaseAsync(trip.DriverId, trip.VehicleId, trip.Id, cancellationToken);
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
