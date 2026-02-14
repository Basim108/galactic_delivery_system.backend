using Microsoft.EntityFrameworkCore;
using Npgsql;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Application.Exceptions;
using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Infrastructure.Persistence.EfCore.Repositories;

public sealed class EfResourceBookingService(SpaceTruckersDbContext db) : IResourceBookingService
{
    public async Task ReserveAsync(DriverId driverId, VehicleId vehicleId, TripId tripId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Keep the same best-effort compensation semantics as the in-memory implementation.
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        await ReserveAsync(ResourceBookingType.Driver, driverId.Value, tripId.Value, cancellationToken);
        await ReserveAsync(ResourceBookingType.Vehicle, vehicleId.Value, tripId.Value, cancellationToken);

        await tx.CommitAsync(cancellationToken);
    }

    public async Task ReleaseAsync(DriverId driverId, VehicleId vehicleId, TripId tripId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await ReleaseAsync(ResourceBookingType.Driver, driverId.Value, tripId.Value, cancellationToken);
        await ReleaseAsync(ResourceBookingType.Vehicle, vehicleId.Value, tripId.Value, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ReserveAsync(
        ResourceBookingType type,
        Guid resourceId,
        Guid tripId,
        CancellationToken cancellationToken)
    {
        var existing = await db.ResourceBookings
            .FirstOrDefaultAsync(x => x.ResourceType == type && x.ResourceId == resourceId, cancellationToken);

        if (existing is not null)
        {
            if (existing.TripId != tripId)
            {
                throw new OptimisticConcurrencyException($"{type} '{resourceId}' is already reserved by another trip.");
            }

            // Idempotent reserve.
            return;
        }

        db.ResourceBookings.Add(new ResourceBooking(type, resourceId, tripId));

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new OptimisticConcurrencyException($"{type} '{resourceId}' reservation concurrency conflict.");
        }
    }

    private async Task ReleaseAsync(
        ResourceBookingType type,
        Guid resourceId,
        Guid tripId,
        CancellationToken cancellationToken)
    {
        var existing = await db.ResourceBookings
            .FirstOrDefaultAsync(x => x.ResourceType == type && x.ResourceId == resourceId, cancellationToken);

        if (existing is null)
        {
            return;
        }

        if (existing.TripId != tripId)
        {
            return;
        }

        db.ResourceBookings.Remove(existing);
    }
}
