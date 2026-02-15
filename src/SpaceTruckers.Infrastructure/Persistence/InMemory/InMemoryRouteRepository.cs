using System.Collections.Concurrent;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Routes;

namespace SpaceTruckers.Infrastructure.Persistence;

public sealed class InMemoryRouteRepository : IRouteRepository
{
    private readonly ConcurrentDictionary<RouteId, Route> _store = new();

    public Task<Route?> GetAsync(RouteId routeId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_store.TryGetValue(routeId, out var route) ? route : null);
    }

    public Task AddAsync(Route route, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store[route.Id] = route;
        return Task.CompletedTask;
    }
}
