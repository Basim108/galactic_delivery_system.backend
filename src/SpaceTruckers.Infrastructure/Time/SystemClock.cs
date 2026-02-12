using SpaceTruckers.Application.Abstractions;

namespace SpaceTruckers.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
