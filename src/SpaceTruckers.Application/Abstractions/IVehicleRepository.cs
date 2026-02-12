using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Resources;

namespace SpaceTruckers.Application.Abstractions;

public interface IVehicleRepository
{
    Task<Vehicle?> GetAsync(VehicleId vehicleId, CancellationToken cancellationToken);

    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken);
}
