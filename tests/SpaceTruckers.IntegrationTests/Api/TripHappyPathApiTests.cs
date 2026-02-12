using FluentAssertions;
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
        created.Status.Should().Be(TripStatus.Planned);

        var started = await client.StartTripAsync(created.TripId);
        started.Status.Should().Be(TripStatus.Active);

        var fetched = await client.GetTripAsync(created.TripId);
        fetched.Status.Should().Be(TripStatus.Active);

        var reachedLuna = await client.ReachCheckpointAsync(created.TripId, "Luna Gate");
        reachedLuna.LastReachedCheckpoint.Should().Be("Luna Gate");

        var reachedMars = await client.ReachCheckpointAsync(created.TripId, "Mars Station");
        reachedMars.LastReachedCheckpoint.Should().Be("Mars Station");

        var summary = await client.CompleteTripAsync(created.TripId);
        summary.Status.Should().Be(TripStatus.Completed);
        summary.CompletedAt.Should().NotBeNull();

        var fetchedSummary = await client.GetTripSummaryAsync(created.TripId);
        fetchedSummary.Status.Should().Be(TripStatus.Completed);
        fetchedSummary.TotalEvents.Should().BeGreaterThanOrEqualTo(summary.TotalEvents);
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

        second.Version.Should().Be(first.Version);
        second.Status.Should().Be(TripStatus.Active);
    }
}
