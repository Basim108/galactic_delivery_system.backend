using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.Infrastructure.Persistence.EfCore;

public sealed class TripEvent
{
    // For EF Core.
    private TripEvent() { }

    public TripEvent(TripId tripId, int sequence, string eventType, DateTimeOffset occurredAt, string payload)
    {
        TripId = tripId;
        Sequence = sequence;
        EventType = eventType;
        OccurredAt = occurredAt;
        Payload = payload;
    }

    public TripId TripId { get; private set; }

    public int Sequence { get; private set; }

    public string EventType { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; private set; }

    public string Payload { get; private set; } = string.Empty;
}
