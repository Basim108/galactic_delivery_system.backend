using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using SpaceTruckers.Application;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Routes;
using SpaceTruckers.Infrastructure.Persistence.EfCore.Repositories;

namespace SpaceTruckers.Infrastructure.Persistence;

public sealed class FeatureFlaggedRouteRepository(
    IFeatureManagerSnapshot featureManager,
    IServiceProvider serviceProvider,
    InMemoryRouteRepository inMemory)
    : IRouteRepository
{
    public async Task<Route?> GetAsync(RouteId routeId, CancellationToken cancellationToken)
    {
        var inner = await GetInnerAsync(cancellationToken);
        return await inner.GetAsync(routeId, cancellationToken);
    }

    public async Task AddAsync(Route route, CancellationToken cancellationToken)
    {
        var inner = await GetInnerAsync(cancellationToken);
        await inner.AddAsync(route, cancellationToken);
    }

    private async Task<IRouteRepository> GetInnerAsync(CancellationToken cancellationToken)
    {
        return await featureManager.IsEnabledAsync(FeatureFlags.USE_DOMAIN_PERSISTENT_STORAGE)
                   ? serviceProvider.GetRequiredService<EfRouteRepository>()
                   : inMemory;
    }
}
