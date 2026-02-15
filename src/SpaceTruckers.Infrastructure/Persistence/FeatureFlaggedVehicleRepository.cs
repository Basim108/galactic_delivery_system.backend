using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using SpaceTruckers.Application;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Resources;
using SpaceTruckers.Infrastructure.Persistence.EfCore.Repositories;

namespace SpaceTruckers.Infrastructure.Persistence;

public sealed class FeatureFlaggedVehicleRepository(
    IFeatureManagerSnapshot featureManager,
    IServiceProvider serviceProvider,
    InMemoryVehicleRepository inMemory)
    : IVehicleRepository
{
    public async Task<Vehicle?> GetAsync(VehicleId vehicleId, CancellationToken cancellationToken)
    {
        var inner = await GetInnerAsync(cancellationToken);
        return await inner.GetAsync(vehicleId, cancellationToken);
    }

    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken)
    {
        var inner = await GetInnerAsync(cancellationToken);
        await inner.AddAsync(vehicle, cancellationToken);
    }

    private async Task<IVehicleRepository> GetInnerAsync(CancellationToken cancellationToken)
    {
        return await featureManager.IsEnabledAsync(FeatureFlags.USE_DOMAIN_PERSISTENT_STORAGE)
                   ? serviceProvider.GetRequiredService<EfVehicleRepository>()
                   : inMemory;
    }
}
