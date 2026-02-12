namespace SpaceTruckers.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
