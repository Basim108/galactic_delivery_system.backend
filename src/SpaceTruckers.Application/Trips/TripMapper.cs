using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.Application.Trips;

public static class TripMapper
{
    public static TripDto ToDto(Trip trip) => new(
        trip.Id.Value,
        trip.DriverId.Value,
        trip.VehicleId.Value,
        trip.RouteId.Value,
        trip.Status,
        trip.Version,
        trip.LastReachedCheckpointName);
}
