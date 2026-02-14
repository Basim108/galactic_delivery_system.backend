using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using SpaceTruckers.Application;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Infrastructure.Persistence.EfCore.Repositories;

namespace SpaceTruckers.Infrastructure.Persistence;

public sealed class FeatureFlaggedResourceBookingService(
    IFeatureManagerSnapshot featureManager,
    IServiceProvider serviceProvider,
    InMemoryResourceBookingService inMemory)
    : IResourceBookingService
{
    public async Task ReserveAsync(DriverId driverId, VehicleId vehicleId, TripId tripId, CancellationToken cancellationToken)
    {
        var inner = await GetInnerAsync(cancellationToken);
        await inner.ReserveAsync(driverId, vehicleId, tripId, cancellationToken);
    }

    public async Task ReleaseAsync(DriverId driverId, VehicleId vehicleId, TripId tripId, CancellationToken cancellationToken)
    {
        var inner = await GetInnerAsync(cancellationToken);
        await inner.ReleaseAsync(driverId, vehicleId, tripId, cancellationToken);
    }

    private async Task<IResourceBookingService> GetInnerAsync(CancellationToken cancellationToken)
    {
        return await featureManager.IsEnabledAsync(FeatureFlags.UseDomainPersistentStorage)
            ? serviceProvider.GetRequiredService<EfResourceBookingService>()
            : inMemory;
    }
}
