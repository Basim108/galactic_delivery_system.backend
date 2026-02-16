using Microsoft.Extensions.Logging;

namespace SpaceTruckers.UnitTests.TestDoubles;

public sealed class TestLogger<T> : ILogger<T>
{
    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose() { }
    }

    public List<LogEntry> Entries { get; } = new();

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
    }

    public sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);
}
