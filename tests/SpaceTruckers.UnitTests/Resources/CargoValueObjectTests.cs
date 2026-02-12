using FluentAssertions;
using SpaceTruckers.Domain.Resources;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.UnitTests.Resources;

public sealed class CargoValueObjectTests
{
    [Fact]
    public void CargoCapacity_FromNegative_Throws()
    {
        var act = () => CargoCapacity.From(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CargoRequirement_FromNegative_Throws()
    {
        var act = () => CargoRequirement.From(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CargoCapacity_ToString_ReturnsValue()
    {
        var capacity = CargoCapacity.From(123);
        capacity.ToString().Should().Be("123");
    }

    [Fact]
    public void CargoRequirement_ToString_ReturnsValue()
    {
        var requirement = CargoRequirement.From(456);
        requirement.ToString().Should().Be("456");
    }
}
