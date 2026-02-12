using System.Net.Http.Json;
using SpaceTruckers.Api.Contracts;
using SpaceTruckers.Application.Trips;
using SpaceTruckers.Domain.Resources;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.IntegrationTests.Api;

public sealed class ApiClient(HttpClient http)
{
    public async Task<Guid> CreateDriverAsync(string name, ResourceStatus status)
    {
        var response = await http.PostAsJsonAsync("/api/drivers", new CreateDriverRequest(name, status));
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<CreateDriverResponse>();
        Assert.NotNull(body);
        return body.DriverId;
    }

    public async Task<Guid> CreateVehicleAsync(string name, string type, int cargoCapacity, ResourceStatus status)
    {
        var response = await http.PostAsJsonAsync("/api/vehicles", new CreateVehicleRequest(name, type, cargoCapacity, status));
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<CreateVehicleResponse>();
        Assert.NotNull(body);
        return body.VehicleId;
    }

    public async Task<Guid> CreateRouteAsync(string name, params string[] checkpoints)
    {
        var response = await http.PostAsJsonAsync("/api/routes", new CreateRouteRequest(name, checkpoints));
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<CreateRouteResponse>();
        Assert.NotNull(body);
        return body.RouteId;
    }

    public async Task<TripDto> CreateTripAsync(Guid driverId, Guid vehicleId, Guid routeId, int cargoRequirement, Guid? tripId = null)
    {
        var response = await http.PostAsJsonAsync(
            "/api/trips",
            new CreateTripRequest(driverId, vehicleId, routeId, cargoRequirement, tripId));

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TripDto>();
        Assert.NotNull(body);
        return body;
    }

    public async Task<TripDto> StartTripAsync(Guid tripId, string? requestId = null)
    {
        var response = await http.PostAsJsonAsync($"/api/trips/{tripId}/start", new StartTripRequest(requestId));
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TripDto>();
        Assert.NotNull(body);
        return body;
    }

    public async Task<TripDto> ReachCheckpointAsync(Guid tripId, string checkpointName)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/trips/{tripId}/checkpoints/reach",
            new ReachCheckpointRequest(checkpointName));

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TripDto>();
        Assert.NotNull(body);
        return body;
    }

    public async Task<TripDto> ReportIncidentAsync(Guid tripId, string type, IncidentSeverity severity)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/trips/{tripId}/incidents",
            new ReportIncidentRequest(type, severity, null));

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TripDto>();
        Assert.NotNull(body);
        return body;
    }

    public async Task<TripSummaryDto> CompleteTripAsync(Guid tripId)
    {
        var response = await http.PostAsync($"/api/trips/{tripId}/complete", content: null);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TripSummaryDto>();
        Assert.NotNull(body);
        return body;
    }

    public async Task<TripDto> GetTripAsync(Guid tripId)
    {
        var response = await http.GetAsync($"/api/trips/{tripId}");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TripDto>();
        Assert.NotNull(body);
        return body;
    }

    public async Task<TripSummaryDto> GetTripSummaryAsync(Guid tripId)
    {
        var response = await http.GetAsync($"/api/trips/{tripId}/summary");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TripSummaryDto>();
        Assert.NotNull(body);
        return body;
    }
}
