using Microsoft.EntityFrameworkCore;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Routes;

namespace SpaceTruckers.Infrastructure.Persistence.EfCore.Repositories;

public sealed class EfRouteRepository(SpaceTruckersDbContext db) : IRouteRepository
{
    public async Task<Route?> GetAsync(RouteId routeId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await db.Routes
            .AsNoTracking()
            .Include("_checkpoints")
            .FirstOrDefaultAsync(x => x.Id == routeId, cancellationToken);
    }

    public async Task AddAsync(Route route, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        db.Routes.Add(route);
        await db.SaveChangesAsync(cancellationToken);
    }
}
