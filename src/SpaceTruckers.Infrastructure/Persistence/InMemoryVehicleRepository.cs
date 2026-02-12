using System.Collections.Concurrent;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Resources;

namespace SpaceTruckers.Infrastructure.Persistence;

public sealed class InMemoryVehicleRepository : IVehicleRepository
{
    private readonly ConcurrentDictionary<VehicleId, Vehicle> _store = new();

    public Task<Vehicle?> GetAsync(VehicleId vehicleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_store.TryGetValue(vehicleId, out var vehicle) ? vehicle : null);
    }

    public Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store[vehicle.Id] = vehicle;
        return Task.CompletedTask;
    }
}
