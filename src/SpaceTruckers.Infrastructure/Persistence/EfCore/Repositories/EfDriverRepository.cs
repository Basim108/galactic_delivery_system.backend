using Microsoft.EntityFrameworkCore;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Resources;

namespace SpaceTruckers.Infrastructure.Persistence.EfCore.Repositories;

public sealed class EfDriverRepository(SpaceTruckersDbContext db) : IDriverRepository
{
    public async Task<Driver?> GetAsync(DriverId driverId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await db.Drivers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == driverId, cancellationToken);
    }

    public async Task AddAsync(Driver driver, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        db.Drivers.Add(driver);
        await db.SaveChangesAsync(cancellationToken);
    }
}
