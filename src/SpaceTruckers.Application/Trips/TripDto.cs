using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.Application.Trips;

public sealed record TripDto(
    Guid TripId,
    Guid DriverId,
    Guid VehicleId,
    Guid RouteId,
    TripStatus Status,
    uint Version,
    string? LastReachedCheckpoint);
