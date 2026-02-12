using SpaceTruckers.Domain.Resources;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.UnitTests.Resources;

public sealed class CargoValueObjectTests
{
    [Fact]
    public void CargoCapacity_FromNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CargoCapacity.From(-1));
    }

    [Fact]
    public void CargoRequirement_FromNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CargoRequirement.From(-1));
    }

    [Fact]
    public void CargoCapacity_ToString_ReturnsValue()
    {
        var capacity = CargoCapacity.From(123);
        Assert.Equal("123", capacity.ToString());
    }

    [Fact]
    public void CargoRequirement_ToString_ReturnsValue()
    {
        var requirement = CargoRequirement.From(456);
        Assert.Equal("456", requirement.ToString());
    }
}
