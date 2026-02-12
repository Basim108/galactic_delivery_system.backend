using FluentAssertions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Routes;

namespace SpaceTruckers.UnitTests.Routes;

public sealed class RouteTests
{
    [Fact]
    public void Create_WithNoCheckpoints_Throws()
    {
        var act = () => Route.Create(RouteId.New(), "R", Array.Empty<string>());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_TrimsCheckpointNames_AndAssignsSequence()
    {
        var route = Route.Create(RouteId.New(), "R", ["  Earth  ", "Mars Station"]);

        route.Checkpoints.Should().HaveCount(2);
        route.Checkpoints[0].Sequence.Should().Be(1);
        route.Checkpoints[0].Name.Should().Be("Earth");
        route.Checkpoints[1].Sequence.Should().Be(2);
    }
}
