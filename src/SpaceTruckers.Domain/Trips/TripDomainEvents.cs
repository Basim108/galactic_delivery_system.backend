using SpaceTruckers.Domain.Common;
using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Domain.Trips;

public sealed record TripCreated(
    TripId TripId,
    DriverId DriverId,
    VehicleId VehicleId,
    RouteId RouteId,
    DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record TripStarted(TripId TripId, DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record CheckpointReached(
    TripId TripId,
    CheckpointId CheckpointId,
    string CheckpointName,
    int Sequence,
    DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record IncidentReported(
    TripId TripId,
    string Type,
    IncidentSeverity Severity,
    string? Description,
    DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record TripCompleted(TripId TripId, DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record TripAborted(
    TripId TripId,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;
