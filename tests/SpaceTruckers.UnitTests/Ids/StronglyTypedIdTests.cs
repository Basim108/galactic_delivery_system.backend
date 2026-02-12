using FluentAssertions;
using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.UnitTests.Ids;

public sealed class StronglyTypedIdTests
{
    [Fact]
    public void TripId_Parse_RoundTrip_Works()
    {
        var id = TripId.New();
        var parsed = TripId.Parse(id.ToString());
        parsed.Should().Be(id);
    }

    [Fact]
    public void TripId_TryParse_ValidGuid_ReturnsTrue()
    {
        var id = TripId.New();
        var result = TripId.TryParse(id.ToString(), out var parsed);
        result.Should().BeTrue();
        parsed.Should().Be(id);
    }

    [Fact]
    public void TripId_TryParse_InvalidGuid_ReturnsFalse()
    {
        var result = TripId.TryParse("not-a-guid", out _);
        result.Should().BeFalse();
    }

    [Fact]
    public void DriverId_Parse_RoundTrip_Works()
    {
        var id = DriverId.New();
        var parsed = DriverId.Parse(id.ToString());
        parsed.Should().Be(id);
    }

    [Fact]
    public void DriverId_TryParse_InvalidGuid_ReturnsFalse()
    {
        var result = DriverId.TryParse("not-a-guid", out _);
        result.Should().BeFalse();
    }

    [Fact]
    public void VehicleId_Parse_RoundTrip_Works()
    {
        var id = VehicleId.New();
        var parsed = VehicleId.Parse(id.ToString());
        parsed.Should().Be(id);
    }

    [Fact]
    public void VehicleId_TryParse_InvalidGuid_ReturnsFalse()
    {
        var result = VehicleId.TryParse("not-a-guid", out _);
        result.Should().BeFalse();
    }

    [Fact]
    public void RouteId_Parse_RoundTrip_Works()
    {
        var id = RouteId.New();
        var parsed = RouteId.Parse(id.ToString());
        parsed.Should().Be(id);
    }

    [Fact]
    public void RouteId_TryParse_InvalidGuid_ReturnsFalse()
    {
        var result = RouteId.TryParse("not-a-guid", out _);
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckpointId_Parse_RoundTrip_Works()
    {
        var id = CheckpointId.New();
        var parsed = CheckpointId.Parse(id.ToString());
        parsed.Should().Be(id);
    }

    [Fact]
    public void CheckpointId_TryParse_ValidGuid_ReturnsTrue()
    {
        var id = CheckpointId.New();
        var result = CheckpointId.TryParse(id.ToString(), out var parsed);
        result.Should().BeTrue();
        parsed.Should().Be(id);
    }

    [Fact]
    public void CheckpointId_TryParse_InvalidGuid_ReturnsFalse()
    {
        var result = CheckpointId.TryParse("not-a-guid", out _);
        result.Should().BeFalse();
    }
}
