using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Application.Abstractions;

public interface IResourceBookingService
{
    Task ReserveAsync(DriverId driverId, VehicleId vehicleId, TripId tripId, CancellationToken cancellationToken);

    Task ReleaseAsync(DriverId driverId, VehicleId vehicleId, TripId tripId, CancellationToken cancellationToken);
}
