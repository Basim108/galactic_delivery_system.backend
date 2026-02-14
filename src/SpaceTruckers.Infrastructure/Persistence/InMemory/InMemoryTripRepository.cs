using System.Collections.Concurrent;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Application.Exceptions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.Infrastructure.Persistence;

public sealed class InMemoryTripRepository : ITripRepository
{
    private readonly ConcurrentDictionary<TripId, Trip> _store = new();

    public Task<Trip?> GetAsync(TripId tripId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(_store.TryGetValue(tripId, out var trip)
            ? trip.CloneForPersistence()
            : null);
    }

    public Task AddAsync(Trip trip, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_store.TryAdd(trip.Id, trip.CloneForPersistence()))
        {
            throw new InvalidOperationException($"Trip '{trip.Id}' already exists.");
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Trip trip, uint expectedVersion, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_store.TryGetValue(trip.Id, out var existing))
        {
            throw new NotFoundException($"Trip '{trip.Id}' was not found.");
        }

        if (existing.Version != expectedVersion)
        {
            throw new OptimisticConcurrencyException(
                $"Trip '{trip.Id}' concurrency conflict. Expected version {expectedVersion} but was {existing.Version}.");
        }

        if (!_store.TryUpdate(trip.Id, trip.CloneForPersistence(), existing))
        {
            throw new OptimisticConcurrencyException($"Trip '{trip.Id}' concurrency conflict.");
        }

        return Task.CompletedTask;
    }
}
