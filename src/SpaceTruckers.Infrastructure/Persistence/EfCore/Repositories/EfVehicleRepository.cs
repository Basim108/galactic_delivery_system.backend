using Microsoft.EntityFrameworkCore;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Resources;

namespace SpaceTruckers.Infrastructure.Persistence.EfCore.Repositories;

public sealed class EfVehicleRepository(SpaceTruckersDbContext db) : IVehicleRepository
{
    public async Task<Vehicle?> GetAsync(VehicleId vehicleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await db.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == vehicleId, cancellationToken);
    }

    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(cancellationToken);
    }
}
