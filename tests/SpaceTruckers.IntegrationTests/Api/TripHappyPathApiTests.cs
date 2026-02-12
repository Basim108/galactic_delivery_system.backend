using SpaceTruckers.Domain.Resources;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.IntegrationTests.Api;

public sealed class TripHappyPathApiTests
{
    [Fact]
    public async Task TripLifecycle_HappyPath_ProducesCompletedSummary()
    {
        await using var factory = new SpaceTruckersApiFactory();
        var client = new ApiClient(factory.CreateClient());

        var driverId = await client.CreateDriverAsync("Driver A", ResourceStatus.Available);
        var vehicleId = await client.CreateVehicleAsync("Vehicle A", "RocketVan", cargoCapacity: 1000, ResourceStatus.Available);
        var routeId = await client.CreateRouteAsync("Route 1", "Earth", "Luna Gate", "Mars Station");

        var created = await client.CreateTripAsync(driverId, vehicleId, routeId, cargoRequirement: 10);
        Assert.Equal(TripStatus.Planned, created.Status);

        var started = await client.StartTripAsync(created.TripId);
        Assert.Equal(TripStatus.Active, started.Status);

        var fetched = await client.GetTripAsync(created.TripId);
        Assert.Equal(TripStatus.Active, fetched.Status);

        var reachedLuna = await client.ReachCheckpointAsync(created.TripId, "Luna Gate");
        Assert.Equal("Luna Gate", reachedLuna.LastReachedCheckpoint);

        var reachedMars = await client.ReachCheckpointAsync(created.TripId, "Mars Station");
        Assert.Equal("Mars Station", reachedMars.LastReachedCheckpoint);

        var summary = await client.CompleteTripAsync(created.TripId);
        Assert.Equal(TripStatus.Completed, summary.Status);
        Assert.NotNull(summary.CompletedAt);

        var fetchedSummary = await client.GetTripSummaryAsync(created.TripId);
        Assert.Equal(TripStatus.Completed, fetchedSummary.Status);
        Assert.True(fetchedSummary.TotalEvents >= summary.TotalEvents);
    }

    [Fact]
    public async Task StartTrip_SameRequestIdTwice_IsIdempotent()
    {
        await using var factory = new SpaceTruckersApiFactory();
        var client = new ApiClient(factory.CreateClient());

        var driverId = await client.CreateDriverAsync("Driver A", ResourceStatus.Available);
        var vehicleId = await client.CreateVehicleAsync("Vehicle A", "RocketVan", cargoCapacity: 1000, ResourceStatus.Available);
        var routeId = await client.CreateRouteAsync("Route 1", "Earth", "Mars Station");

        var created = await client.CreateTripAsync(driverId, vehicleId, routeId, cargoRequirement: 10);

        var first = await client.StartTripAsync(created.TripId, requestId: "REQ-1");
        var second = await client.StartTripAsync(created.TripId, requestId: "REQ-1");

        Assert.Equal(first.Version, second.Version);
        Assert.Equal(TripStatus.Active, second.Status);
    }
}
