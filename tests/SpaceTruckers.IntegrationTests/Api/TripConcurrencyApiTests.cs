using System.Net;
using System.Net.Http.Json;
using SpaceTruckers.Api.Contracts;
using SpaceTruckers.Domain.Resources;

namespace SpaceTruckers.IntegrationTests.Api;

public sealed class TripConcurrencyApiTests
{
    [Fact]
    public async Task TwoConcurrentTripCreations_WithSameDriverAndVehicle_OneFailsWithConcurrency()
    {
        await using var factory = new SpaceTruckersApiFactory();
        var http = factory.CreateClient();
        var client = new ApiClient(http);

        var driverId = await client.CreateDriverAsync("Driver A", ResourceStatus.Available);
        var vehicleId = await client.CreateVehicleAsync("Vehicle A", "RocketVan", cargoCapacity: 1000, ResourceStatus.Available);
        var routeId1 = await client.CreateRouteAsync("Route 1", "Earth", "Mars Station");
        var routeId2 = await client.CreateRouteAsync("Route 2", "Earth", "Luna Gate", "Mars Station");

        var req1 = new CreateTripRequest(driverId, vehicleId, routeId1, CargoRequirement: 10, TripId: null);
        var req2 = new CreateTripRequest(driverId, vehicleId, routeId2, CargoRequirement: 10, TripId: null);

        var t1 = http.PostAsJsonAsync("/api/trips", req1);
        var t2 = http.PostAsJsonAsync("/api/trips", req2);

        var responses = await Task.WhenAll(t1, t2);

        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        Assert.Equal(1, successCount);
        Assert.Equal(1, conflictCount);

        var conflict = responses.Single(r => r.StatusCode == HttpStatusCode.Conflict);
        var problem = await conflict.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.NotNull(problem);
        Assert.Equal("OptimisticConcurrencyException", problem.Code);
    }

    [Fact]
    public async Task TwoConcurrentCheckpointUpdates_OnSameTrip_OneFailsWithConcurrency()
    {
        await using var factory = new SpaceTruckersApiFactory();
        var http = factory.CreateClient();
        var client = new ApiClient(http);

        var driverId = await client.CreateDriverAsync("Driver A", ResourceStatus.Available);
        var vehicleId = await client.CreateVehicleAsync("Vehicle A", "RocketVan", cargoCapacity: 1000, ResourceStatus.Available);
        var routeId = await client.CreateRouteAsync("Route 1", "Earth", "Luna Gate", "Mars Station");

        var trip = await client.CreateTripAsync(driverId, vehicleId, routeId, cargoRequirement: 10);
        await client.StartTripAsync(trip.TripId);

        var request = new ReachCheckpointRequest("Luna Gate");

        var t1 = http.PostAsJsonAsync($"/api/trips/{trip.TripId}/checkpoints/reach", request);
        var t2 = http.PostAsJsonAsync($"/api/trips/{trip.TripId}/checkpoints/reach", request);

        var responses = await Task.WhenAll(t1, t2);

        var okCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        Assert.Equal(1, okCount);
        Assert.Equal(1, conflictCount);

        var conflict = responses.Single(r => r.StatusCode == HttpStatusCode.Conflict);
        var problem = await conflict.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.NotNull(problem);
        Assert.Equal("OptimisticConcurrencyException", problem.Code);
    }
}
