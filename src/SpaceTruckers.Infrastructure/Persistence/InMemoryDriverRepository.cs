using System.Collections.Concurrent;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Resources;

namespace SpaceTruckers.Infrastructure.Persistence;

public sealed class InMemoryDriverRepository : IDriverRepository
{
    private readonly ConcurrentDictionary<DriverId, Driver> _store = new();

    public Task<Driver?> GetAsync(DriverId driverId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_store.TryGetValue(driverId, out var driver) ? driver : null);
    }

    public Task AddAsync(Driver driver, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store[driver.Id] = driver;
        return Task.CompletedTask;
    }
}
