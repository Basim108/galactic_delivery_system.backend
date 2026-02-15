using FluentAssertions;
using MediatR;
using SpaceTruckers.Application.Behaviors;
using SpaceTruckers.UnitTests.TestDoubles;

namespace SpaceTruckers.UnitTests.Application.Behaviors;

public sealed class LoggingBehaviorTests
{
    private sealed record DummyRequest : IRequest<string>;

    [Fact]
    public async Task Handle_NextSucceeds_LogsStartAndEnd()
    {
        var logger = new TestLogger<LoggingBehavior<DummyRequest, string>>();
        var behavior = new LoggingBehavior<DummyRequest, string>(logger);

        var response = await behavior.Handle(
            new DummyRequest(),
            next: _ => Task.FromResult("ok"),
            cancellationToken: CancellationToken.None);

        response.Should().Be("ok");

        logger.Entries.Select(x => (x.Level, x.Message)).Should().ContainInOrder(
            (Microsoft.Extensions.Logging.LogLevel.Trace, "DummyRequest starts"),
            (Microsoft.Extensions.Logging.LogLevel.Trace, "DummyRequest ends"));
    }

    [Fact]
    public async Task Handle_NextThrows_LogsStartAndEnd()
    {
        var logger = new TestLogger<LoggingBehavior<DummyRequest, string>>();
        var behavior = new LoggingBehavior<DummyRequest, string>(logger);

        var act = async () =>
        {
            await behavior.Handle(
                new DummyRequest(),
                next: _ => throw new InvalidOperationException("boom"),
                cancellationToken: CancellationToken.None);
        };

        await act.Should().ThrowAsync<InvalidOperationException>();

        logger.Entries.Select(x => (x.Level, x.Message)).Should().ContainInOrder(
            (Microsoft.Extensions.Logging.LogLevel.Trace, "DummyRequest starts"),
            (Microsoft.Extensions.Logging.LogLevel.Trace, "DummyRequest ends"));
    }
}
