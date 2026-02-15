using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using SpaceTruckers.Application;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Resources;
using SpaceTruckers.Infrastructure.Persistence.EfCore.Repositories;

namespace SpaceTruckers.Infrastructure.Persistence;

public sealed class FeatureFlaggedDriverRepository(
    IFeatureManagerSnapshot featureManager,
    IServiceProvider serviceProvider,
    InMemoryDriverRepository inMemory)
    : IDriverRepository
{
    public async Task<Driver?> GetAsync(DriverId driverId, CancellationToken cancellationToken)
    {
        var inner = await GetInnerAsync(cancellationToken);
        return await inner.GetAsync(driverId, cancellationToken);
    }

    public async Task AddAsync(Driver driver, CancellationToken cancellationToken)
    {
        var inner = await GetInnerAsync(cancellationToken);
        await inner.AddAsync(driver, cancellationToken);
    }

    private async Task<IDriverRepository> GetInnerAsync(CancellationToken cancellationToken)
    {
        return await featureManager.IsEnabledAsync(FeatureFlags.USE_DOMAIN_PERSISTENT_STORAGE)
                   ? serviceProvider.GetRequiredService<EfDriverRepository>()
                   : inMemory;
    }
}
