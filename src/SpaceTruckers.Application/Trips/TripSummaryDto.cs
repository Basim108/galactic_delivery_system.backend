using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.Application.Trips;

public sealed record TripSummaryDto(
    Guid TripId,
    TripStatus Status,
    int TotalEvents,
    int TotalIncidents,
    string? LastReachedCheckpoint,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? AbortedAt);
