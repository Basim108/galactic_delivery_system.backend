using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Routes;

namespace SpaceTruckers.Application.Abstractions;

public interface IRouteRepository
{
    Task<Route?> GetAsync(RouteId routeId, CancellationToken cancellationToken);

    Task AddAsync(Route route, CancellationToken cancellationToken);
}
