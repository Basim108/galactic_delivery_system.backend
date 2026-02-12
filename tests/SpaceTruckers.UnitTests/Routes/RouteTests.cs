using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Routes;

namespace SpaceTruckers.UnitTests.Routes;

public sealed class RouteTests
{
    [Fact]
    public void Create_WithNoCheckpoints_Throws()
    {
        Assert.Throws<ArgumentException>(() => Route.Create(RouteId.New(), "R", Array.Empty<string>()));
    }

    [Fact]
    public void Create_TrimsCheckpointNames_AndAssignsSequence()
    {
        var route = Route.Create(RouteId.New(), "R", ["  Earth  ", "Mars Station"]);

        Assert.Equal(2, route.Checkpoints.Count);
        Assert.Equal(1, route.Checkpoints[0].Sequence);
        Assert.Equal("Earth", route.Checkpoints[0].Name);
        Assert.Equal(2, route.Checkpoints[1].Sequence);
    }
}
