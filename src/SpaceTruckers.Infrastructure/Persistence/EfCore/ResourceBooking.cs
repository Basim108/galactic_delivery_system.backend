namespace SpaceTruckers.Infrastructure.Persistence.EfCore;

public enum ResourceBookingType
{
    Driver = 0,
    Vehicle = 1,
}

public sealed class ResourceBooking
{
    // For EF Core.
    private ResourceBooking() { }

    public ResourceBooking(ResourceBookingType resourceType, Guid resourceId, Guid tripId)
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
        TripId = tripId;
    }

    public ResourceBookingType ResourceType { get; private set; }

    public Guid ResourceId { get; private set; }

    public Guid TripId { get; private set; }
}
