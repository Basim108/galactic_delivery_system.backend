namespace SpaceTruckers.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
