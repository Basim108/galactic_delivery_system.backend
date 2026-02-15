using System.Collections.Concurrent;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Application.Exceptions;
using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Infrastructure.Persistence;

public sealed class InMemoryResourceBookingService : IResourceBookingService
{
    private sealed record Booking(int Version, TripId? TripId);

    private readonly ConcurrentDictionary<DriverId, Booking> _driverBookings = new();
    private readonly ConcurrentDictionary<VehicleId, Booking> _vehicleBookings = new();

    public async Task ReserveAsync(DriverId driverId, VehicleId vehicleId, TripId tripId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Reserve(_driverBookings, driverId, tripId, "Driver");

        try
        {
            Reserve(_vehicleBookings, vehicleId, tripId, "Vehicle");
        }
        catch
        {
            // Best-effort compensation.
            Release(_driverBookings, driverId, tripId, "Driver");
            throw;
        }

        await Task.CompletedTask;
    }

    public Task ReleaseAsync(DriverId driverId, VehicleId vehicleId, TripId tripId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Release(_driverBookings, driverId, tripId, "Driver");
        Release(_vehicleBookings, vehicleId, tripId, "Vehicle");

        return Task.CompletedTask;
    }

    private static void Reserve<TId>(ConcurrentDictionary<TId, Booking> store, TId id, TripId tripId, string resourceType)
        where TId : notnull
    {
        var current = store.GetOrAdd(id, _ => new Booking(0, null));

        if (current.TripId is not null && current.TripId.Value != tripId)
        {
            throw new OptimisticConcurrencyException($"{resourceType} '{id}' is already reserved by another trip.");
        }

        var updated = current with { Version = current.Version + 1, TripId = tripId };

        if (!store.TryUpdate(id, updated, current))
        {
            throw new OptimisticConcurrencyException($"{resourceType} '{id}' reservation concurrency conflict.");
        }
    }

    private static void Release<TId>(ConcurrentDictionary<TId, Booking> store, TId id, TripId tripId, string resourceType)
        where TId : notnull
    {
        if (!store.TryGetValue(id, out var current))
        {
            return;
        }

        if (current.TripId is null || current.TripId.Value != tripId)
        {
            return;
        }

        var updated = current with { Version = current.Version + 1, TripId = null };

        if (!store.TryUpdate(id, updated, current))
        {
            throw new OptimisticConcurrencyException($"{resourceType} '{id}' release concurrency conflict.");
        }
    }
}
