using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Application.Exceptions;
using SpaceTruckers.Domain.Common;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Trips;
using SpaceTruckers.Infrastructure.Persistence.EfCore;

namespace SpaceTruckers.Infrastructure.Persistence.EfCore.Repositories;

public sealed class EfTripRepository(SpaceTruckersDbContext db) : ITripRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly IReadOnlyDictionary<string, Type> EventTypeMap = new Dictionary<string, Type>(StringComparer.Ordinal)
    {
        [typeof(TripCreated).FullName!] = typeof(TripCreated),
        [typeof(TripStarted).FullName!] = typeof(TripStarted),
        [typeof(CheckpointReached).FullName!] = typeof(CheckpointReached),
        [typeof(IncidentReported).FullName!] = typeof(IncidentReported),
        [typeof(TripCompleted).FullName!] = typeof(TripCompleted),
        [typeof(TripAborted).FullName!] = typeof(TripAborted),
    };

    public async Task<Trip?> GetAsync(TripId tripId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var trip = await db.Trips
            .AsNoTracking()
            .Include("_checkpoints")
            .FirstOrDefaultAsync(x => x.Id == tripId, cancellationToken);

        if (trip is null)
        {
            return null;
        }

        var recordedEvents = await db.TripEvents
            .AsNoTracking()
            .Where(e => e.TripId == tripId)
            .OrderBy(e => e.Sequence)
            .Select(e => DeserializeEvent(e.EventType, e.Payload))
            .ToListAsync(cancellationToken);

        trip.RestoreRecordedEvents(recordedEvents);

        return trip.CloneForPersistence();
    }

    public async Task AddAsync(Trip trip, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        db.Trips.Add(trip);

        AppendNewRecordedEvents(trip, existingEventCount: 0);

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Trip trip, uint expectedVersion, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existing = await db.Trips
            .Include("_checkpoints")
            .FirstOrDefaultAsync(x => x.Id == trip.Id, cancellationToken);

        if (existing is null)
        {
            throw new NotFoundException($"Trip '{trip.Id}' was not found.");
        }

        var tripId = trip.Id;
        var existingEventCount = await db.TripEvents
            .CountAsync(e => e.TripId == tripId, cancellationToken);

        // Copy scalar state.
        db.Entry(existing).CurrentValues.SetValues(trip);

        // Apply expected version for optimistic concurrency.
        // (Must be set after SetValues to avoid SetValues overwriting the original value.)
        db.Entry(existing).Property(x => x.Version).OriginalValue = expectedVersion;

        // Copy idempotency keys.
        db.Entry(existing).Property("_startTripRequestIds").CurrentValue =
            trip.StartTripRequestIds.ToHashSet(StringComparer.Ordinal);

        // Persist newly recorded events.
        AppendNewRecordedEvents(trip, existingEventCount);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new OptimisticConcurrencyException($"Trip '{trip.Id}' concurrency conflict.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: Npgsql.PostgresErrorCodes.UniqueViolation })
        {
            // When two concurrent updates attempt to append the same next event sequence number,
            // treat the unique violation as an optimistic concurrency conflict.
            throw new OptimisticConcurrencyException($"Trip '{trip.Id}' concurrency conflict.");
        }
    }

    private void AppendNewRecordedEvents(Trip trip, int existingEventCount)
    {
        if (trip.RecordedEvents.Count <= existingEventCount)
        {
            return;
        }

        var newEvents = trip.RecordedEvents
            .Skip(existingEventCount)
            .ToArray();

        for (var i = 0; i < newEvents.Length; i++)
        {
            var domainEvent = newEvents[i];
            var eventType = domainEvent.GetType().FullName ?? domainEvent.GetType().Name;
            var occurredAt = GetOccurredAt(domainEvent);
            var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions);

            db.TripEvents.Add(new TripEvent(
                tripId: trip.Id,
                sequence: existingEventCount + i + 1,
                eventType: eventType,
                occurredAt: occurredAt,
                payload: payload));
        }
    }

    private static IDomainEvent DeserializeEvent(string eventType, string payload)
    {
        if (!EventTypeMap.TryGetValue(eventType, out var concreteType))
        {
            throw new InvalidOperationException($"Unsupported trip event type '{eventType}'.");
        }

        return (IDomainEvent?)JsonSerializer.Deserialize(payload, concreteType, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize trip event '{eventType}'.");
    }

    private static DateTimeOffset GetOccurredAt(IDomainEvent domainEvent) => domainEvent switch
    {
        TripCreated e => e.OccurredAt,
        TripStarted e => e.OccurredAt,
        CheckpointReached e => e.OccurredAt,
        IncidentReported e => e.OccurredAt,
        TripCompleted e => e.OccurredAt,
        TripAborted e => e.OccurredAt,
        _ => throw new InvalidOperationException($"Event type '{domainEvent.GetType().FullName}' does not expose OccurredAt.")
    };
}
