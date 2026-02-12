using SpaceTruckers.Domain.Ids;

namespace SpaceTruckers.UnitTests.Ids;

public sealed class StronglyTypedIdTests
{
    [Fact]
    public void TripId_Parse_RoundTrip_Works()
    {
        var id = TripId.New();
        var parsed = TripId.Parse(id.ToString());
        Assert.Equal(id, parsed);
    }

    [Fact]
    public void TripId_TryParse_ValidGuid_ReturnsTrue()
    {
        var id = TripId.New();
        Assert.True(TripId.TryParse(id.ToString(), out var parsed));
        Assert.Equal(id, parsed);
    }

    [Fact]
    public void TripId_TryParse_InvalidGuid_ReturnsFalse()
    {
        Assert.False(TripId.TryParse("not-a-guid", out _));
    }

    [Fact]
    public void DriverId_Parse_RoundTrip_Works()
    {
        var id = DriverId.New();
        var parsed = DriverId.Parse(id.ToString());
        Assert.Equal(id, parsed);
    }

    [Fact]
    public void DriverId_TryParse_InvalidGuid_ReturnsFalse()
    {
        Assert.False(DriverId.TryParse("not-a-guid", out _));
    }

    [Fact]
    public void VehicleId_Parse_RoundTrip_Works()
    {
        var id = VehicleId.New();
        var parsed = VehicleId.Parse(id.ToString());
        Assert.Equal(id, parsed);
    }

    [Fact]
    public void VehicleId_TryParse_InvalidGuid_ReturnsFalse()
    {
        Assert.False(VehicleId.TryParse("not-a-guid", out _));
    }

    [Fact]
    public void RouteId_Parse_RoundTrip_Works()
    {
        var id = RouteId.New();
        var parsed = RouteId.Parse(id.ToString());
        Assert.Equal(id, parsed);
    }

    [Fact]
    public void RouteId_TryParse_InvalidGuid_ReturnsFalse()
    {
        Assert.False(RouteId.TryParse("not-a-guid", out _));
    }

    [Fact]
    public void CheckpointId_Parse_RoundTrip_Works()
    {
        var id = CheckpointId.New();
        var parsed = CheckpointId.Parse(id.ToString());
        Assert.Equal(id, parsed);
    }

    [Fact]
    public void CheckpointId_TryParse_ValidGuid_ReturnsTrue()
    {
        var id = CheckpointId.New();
        Assert.True(CheckpointId.TryParse(id.ToString(), out var parsed));
        Assert.Equal(id, parsed);
    }

    [Fact]
    public void CheckpointId_TryParse_InvalidGuid_ReturnsFalse()
    {
        Assert.False(CheckpointId.TryParse("not-a-guid", out _));
    }
}
