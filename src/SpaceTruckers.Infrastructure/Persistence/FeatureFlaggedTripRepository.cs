using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using SpaceTruckers.Application;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Trips;
using SpaceTruckers.Infrastructure.Persistence.EfCore.Repositories;

namespace SpaceTruckers.Infrastructure.Persistence;

public sealed class FeatureFlaggedTripRepository(
    IFeatureManagerSnapshot featureManager,
    IServiceProvider serviceProvider,
    InMemoryTripRepository inMemory)
    : ITripRepository
{
    public async Task<Trip?> GetAsync(TripId tripId, CancellationToken cancellationToken)
    {
        var inner = await GetInnerAsync(cancellationToken);
        return await inner.GetAsync(tripId, cancellationToken);
    }

    public async Task AddAsync(Trip trip, CancellationToken cancellationToken)
    {
        var inner = await GetInnerAsync(cancellationToken);
        await inner.AddAsync(trip, cancellationToken);
    }

    public async Task UpdateAsync(Trip trip, uint expectedVersion, CancellationToken cancellationToken)
    {
        var inner = await GetInnerAsync(cancellationToken);
        await inner.UpdateAsync(trip, expectedVersion, cancellationToken);
    }

    private async Task<ITripRepository> GetInnerAsync(CancellationToken cancellationToken)
    {
        return await featureManager.IsEnabledAsync(FeatureFlags.USE_DOMAIN_PERSISTENT_STORAGE)
                   ? serviceProvider.GetRequiredService<EfTripRepository>()
                   : inMemory;
    }
}
