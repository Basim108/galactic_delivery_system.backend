using SpaceTruckers.Domain.Common;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.Application.Trips;

public static class TripSummaryMapper
{
    public static TripSummaryDto From(Trip trip)
    {
        var events = trip.RecordedEvents;
        var totalIncidents = events.OfType<IncidentReported>().Count();

        DateTimeOffset? completedAt = events.OfType<TripCompleted>().Select(e => e.OccurredAt).SingleOrDefault();
        DateTimeOffset? abortedAt = events.OfType<TripAborted>().Select(e => e.OccurredAt).SingleOrDefault();

        return new TripSummaryDto(
            trip.Id.Value,
            trip.Status,
            events.Count,
            totalIncidents,
            trip.LastReachedCheckpointName,
            completedAt,
            abortedAt);
    }
}
