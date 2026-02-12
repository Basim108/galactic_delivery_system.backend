using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SpaceTruckers.Api.Contracts;
using SpaceTruckers.Domain.Common;
using SpaceTruckers.Domain.Resources;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.IntegrationTests.Api;

public sealed class TripEdgeCasesApiTests
{
    [Fact]
    public async Task StartTrip_InsufficientCargoCapacity_ReturnsConflictWithCode()
    {
        await using var factory = new SpaceTruckersApiFactory();
        var http = factory.CreateClient();
        var client = new ApiClient(http);

        var driverId = await client.CreateDriverAsync("Driver A", ResourceStatus.Available);
        var vehicleId = await client.CreateVehicleAsync("Vehicle A", "SpaceCycle", cargoCapacity: 100, ResourceStatus.Available);
        var routeId = await client.CreateRouteAsync("Route 1", "Earth", "Mars Station");

        var trip = await client.CreateTripAsync(driverId, vehicleId, routeId, cargoRequirement: 250);

        var response = await http.PostAsJsonAsync($"/api/trips/{trip.TripId}/start", new StartTripRequest(RequestId: null));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        problem.Should().NotBeNull();
        problem!.Code.Should().Be(DomainErrorCodes.INSUFFICIENT_CARGO_CAPACITY);
    }

    [Fact]
    public async Task ReportIncident_AfterCompletion_IsRejected()
    {
        await using var factory = new SpaceTruckersApiFactory();
        var http = factory.CreateClient();
        var client = new ApiClient(http);

        var driverId = await client.CreateDriverAsync("Driver A", ResourceStatus.Available);
        var vehicleId = await client.CreateVehicleAsync("Vehicle A", "RocketVan", cargoCapacity: 1000, ResourceStatus.Available);
        var routeId = await client.CreateRouteAsync("Route 1", "Earth", "Mars Station");

        var trip = await client.CreateTripAsync(driverId, vehicleId, routeId, cargoRequirement: 10);
        await client.StartTripAsync(trip.TripId);
        await client.ReachCheckpointAsync(trip.TripId, "Mars Station");
        await client.CompleteTripAsync(trip.TripId);

        var response = await http.PostAsJsonAsync(
            $"/api/trips/{trip.TripId}/incidents",
            new ReportIncidentRequest("AsteroidField", IncidentSeverity.Catastrophic, null));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        problem.Should().NotBeNull();
        problem!.Code.Should().Be(DomainErrorCodes.TRIP_ALREADY_COMPLETED);
    }

    [Fact]
    public async Task CatastrophicIncident_AbortsTrip_AndCompletionIsRejected()
    {
        await using var factory = new SpaceTruckersApiFactory();
        var http = factory.CreateClient();
        var client = new ApiClient(http);

        var driverId = await client.CreateDriverAsync("Driver A", ResourceStatus.Available);
        var vehicleId = await client.CreateVehicleAsync("Vehicle A", "RocketVan", cargoCapacity: 1000, ResourceStatus.Available);
        var routeId = await client.CreateRouteAsync("Route 1", "Earth", "Luna Gate", "Mars Station");

        var trip = await client.CreateTripAsync(driverId, vehicleId, routeId, cargoRequirement: 10);
        await client.StartTripAsync(trip.TripId);

        var aborted = await client.ReportIncidentAsync(trip.TripId, "CosmicStorm", IncidentSeverity.Catastrophic);
        aborted.Status.Should().Be(TripStatus.Aborted);

        var response = await http.PostAsync($"/api/trips/{trip.TripId}/complete", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        problem.Should().NotBeNull();
        problem!.Code.Should().Be(DomainErrorCodes.TRIP_NOT_COMPLETABLE);
    }
}
