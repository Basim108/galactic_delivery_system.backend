using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SpaceTruckers.Api.Contracts;
using SpaceTruckers.Domain.Resources;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.IntegrationTests.Api;

public sealed class TripApiValidationTests : IClassFixture<SpaceTruckersApiFactory>
{
    private readonly HttpClient _http;
    private readonly ApiClient _client;

    public TripApiValidationTests(SpaceTruckersApiFactory factory)
    {
        _http = factory.CreateClient();
        _client = new ApiClient(_http);
    }

    [Fact]
    public async Task CreateTrip_WithValidData_ShouldSucceed()
    {
        // Arrange
        var driverId  = await _client.CreateDriverAsync("Driver", ResourceStatus.Available);
        var vehicleId = await _client.CreateVehicleAsync("Vehicle", "Type", 1000, ResourceStatus.Available);
        var routeId   = await _client.CreateRouteAsync("Route", "Checkpoint A", "Checkpoint B");
        var request   = new CreateTripRequest(driverId, vehicleId, routeId, 500, null);

        // Act
        var response = await _http.PostAsJsonAsync("/api/trips", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateTrip_WithEmptyDriverId_ShouldReturnBadRequest()
    {
        // Arrange
        var vehicleId = await _client.CreateVehicleAsync("Vehicle", "Type", 1000, ResourceStatus.Available);
        var routeId   = await _client.CreateRouteAsync("Route", "Checkpoint A");
        var request   = new CreateTripRequest(Guid.Empty, vehicleId, routeId, 500, null);

        // Act
        var response = await _http.PostAsJsonAsync("/api/trips", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTrip_WithEmptyVehicleId_ShouldReturnBadRequest()
    {
        // Arrange
        var driverId = await _client.CreateDriverAsync("Driver", ResourceStatus.Available);
        var routeId  = await _client.CreateRouteAsync("Route", "Checkpoint A");
        var request  = new CreateTripRequest(driverId, Guid.Empty, routeId, 500, null);

        // Act
        var response = await _http.PostAsJsonAsync("/api/trips", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTrip_WithEmptyRouteId_ShouldReturnBadRequest()
    {
        // Arrange
        var driverId  = await _client.CreateDriverAsync("Driver", ResourceStatus.Available);
        var vehicleId = await _client.CreateVehicleAsync("Vehicle", "Type", 1000, ResourceStatus.Available);
        var request   = new CreateTripRequest(driverId, vehicleId, Guid.Empty, 500, null);

        // Act
        var response = await _http.PostAsJsonAsync("/api/trips", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTrip_WithNegativeCargoRequirement_ShouldReturnBadRequest()
    {
        // Arrange
        var driverId  = await _client.CreateDriverAsync("Driver", ResourceStatus.Available);
        var vehicleId = await _client.CreateVehicleAsync("Vehicle", "Type", 1000, ResourceStatus.Available);
        var routeId   = await _client.CreateRouteAsync("Route", "Checkpoint A");
        var request   = new CreateTripRequest(driverId, vehicleId, routeId, -1, null);

        // Act
        var response = await _http.PostAsJsonAsync("/api/trips", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTrip_WithZeroCargoRequirement_ShouldSucceed()
    {
        // Arrange
        var driverId  = await _client.CreateDriverAsync("Driver", ResourceStatus.Available);
        var vehicleId = await _client.CreateVehicleAsync("Vehicle", "Type", 1000, ResourceStatus.Available);
        var routeId   = await _client.CreateRouteAsync("Route", "Checkpoint A");
        var request   = new CreateTripRequest(driverId, vehicleId, routeId, 0, null);

        // Act
        var response = await _http.PostAsJsonAsync("/api/trips", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task StartTrip_WithoutRequestId_ShouldSucceed()
    {
        // Arrange
        var driverId = await _client.CreateDriverAsync("Driver", ResourceStatus.Available);
        var vehicleId = await _client.CreateVehicleAsync("Vehicle", "Type", 1000, ResourceStatus.Available);
        var routeId = await _client.CreateRouteAsync("Route", "Checkpoint A");
        var trip = await _client.CreateTripAsync(driverId, vehicleId, routeId, 500);
        var request = new StartTripRequest(null);

        // Act
        var response = await _http.PostAsJsonAsync($"/api/trips/{trip.TripId}/start", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task StartTrip_WithValidRequestId_ShouldSucceed()
    {
        // Arrange
        var driverId = await _client.CreateDriverAsync("Driver", ResourceStatus.Available);
        var vehicleId = await _client.CreateVehicleAsync("Vehicle", "Type", 1000, ResourceStatus.Available);
        var routeId = await _client.CreateRouteAsync("Route", "Checkpoint A");
        var trip = await _client.CreateTripAsync(driverId, vehicleId, routeId, 500);
        var request = new StartTripRequest("request-123");

        // Act
        var response = await _http.PostAsJsonAsync($"/api/trips/{trip.TripId}/start", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task StartTrip_WithRequestIdExceedingMaxLength_ShouldReturnBadRequest()
    {
        // Arrange
        var driverId = await _client.CreateDriverAsync("Driver", ResourceStatus.Available);
        var vehicleId = await _client.CreateVehicleAsync("Vehicle", "Type", 1000, ResourceStatus.Available);
        var routeId = await _client.CreateRouteAsync("Route", "Checkpoint A");
        var trip = await _client.CreateTripAsync(driverId, vehicleId, routeId, 500);
        var longRequestId = new string('a', 101); // MAX_REQUEST_ID_LENGTH is 100
        var request = new StartTripRequest(longRequestId);

        // Act
        var response = await _http.PostAsJsonAsync($"/api/trips/{trip.TripId}/start", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task StartTrip_WithRequestIdAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var driverId = await _client.CreateDriverAsync("Driver", ResourceStatus.Available);
        var vehicleId = await _client.CreateVehicleAsync("Vehicle", "Type", 1000, ResourceStatus.Available);
        var routeId = await _client.CreateRouteAsync("Route", "Checkpoint A");
        var trip = await _client.CreateTripAsync(driverId, vehicleId, routeId, 500);
        var maxLengthRequestId = new string('a', 100); // MAX_REQUEST_ID_LENGTH is 100
        var request = new StartTripRequest(maxLengthRequestId);

        // Act
        var response = await _http.PostAsJsonAsync($"/api/trips/{trip.TripId}/start", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReachCheckpoint_WithValidCheckpointName_ShouldSucceed()
    {
        // Arrange
        var driverId = await _client.CreateDriverAsync("Driver", ResourceStatus.Available);
        var vehicleId = await _client.CreateVehicleAsync("Vehicle", "Type", 1000, ResourceStatus.Available);
        var routeId = await _client.CreateRouteAsync("Route", "Checkpoint A", "Checkpoint B");
        var trip = await _client.CreateTripAsync(driverId, vehicleId, routeId, 500);
        await _client.StartTripAsync(trip.TripId);
        var request = new ReachCheckpointRequest("Checkpoint A");

        // Act
        var response = await _http.PostAsJsonAsync($"/api/trips/{trip.TripId}/checkpoints/reach", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReachCheckpoint_WithEmptyCheckpointName_ShouldReturnBadRequest()
    {
        // Arrange
        var driverId = await _client.CreateDriverAsync("Driver", ResourceStatus.Available);
        var vehicleId = await _client.CreateVehicleAsync("Vehicle", "Type", 1000, ResourceStatus.Available);
        var routeId = await _client.CreateRouteAsync("Route", "Checkpoint A");
        var trip = await _client.CreateTripAsync(driverId, vehicleId, routeId, 500);
        await _client.StartTripAsync(trip.TripId);
        var request = new ReachCheckpointRequest("");

        // Act
        var response = await _http.PostAsJsonAsync($"/api/trips/{trip.TripId}/checkpoints/reach", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReachCheckpoint_WithCheckpointNameExceedingMaxLength_ShouldReturnBadRequest()
    {
        // Arrange
        var driverId = await _client.CreateDriverAsync("Driver", ResourceStatus.Available);
        var vehicleId = await _client.CreateVehicleAsync("Vehicle", "Type", 1000, ResourceStatus.Available);
        var routeId = await _client.CreateRouteAsync("Route", "Checkpoint A");
        var trip = await _client.CreateTripAsync(driverId, vehicleId, routeId, 500);
        await _client.StartTripAsync(trip.TripId);
        var longCheckpoint = new string('a', 201); // MAX_NAME_LENGTH is 200
        var request = new ReachCheckpointRequest(longCheckpoint);

        // Act
        var response = await _http.PostAsJsonAsync($"/api/trips/{trip.TripId}/checkpoints/reach", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReachCheckpoint_WithCheckpointNameAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var driverId = await _client.CreateDriverAsync("Driver", ResourceStatus.Available);
        var vehicleId = await _client.CreateVehicleAsync("Vehicle", "Type", 1000, ResourceStatus.Available);
        var maxLengthCheckpoint = new string('a', 200); // MAX_NAME_LENGTH is 200
        var routeId = await _client.CreateRouteAsync("Route", maxLengthCheckpoint);
        var trip = await _client.CreateTripAsync(driverId, vehicleId, routeId, 500);
        await _client.StartTripAsync(trip.TripId);
        var request = new ReachCheckpointRequest(maxLengthCheckpoint);

        // Act
        var response = await _http.PostAsJsonAsync($"/api/trips/{trip.TripId}/checkpoints/reach", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReportIncident_WithValidType_ShouldSucceed()
    {
        // Arrange
        var driverId = await _client.CreateDriverAsync("Driver", ResourceStatus.Available);
        var vehicleId = await _client.CreateVehicleAsync("Vehicle", "Type", 1000, ResourceStatus.Available);
        var routeId = await _client.CreateRouteAsync("Route", "Checkpoint A");
        var trip = await _client.CreateTripAsync(driverId, vehicleId, routeId, 500);
        await _client.StartTripAsync(trip.TripId);
        var request = new ReportIncidentRequest("Engine Failure", IncidentSeverity.Catastrophic, null);

        // Act
        var response = await _http.PostAsJsonAsync($"/api/trips/{trip.TripId}/incidents", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReportIncident_WithEmptyType_ShouldReturnBadRequest()
    {
        // Arrange
        var driverId = await _client.CreateDriverAsync("Driver", ResourceStatus.Available);
        var vehicleId = await _client.CreateVehicleAsync("Vehicle", "Type", 1000, ResourceStatus.Available);
        var routeId = await _client.CreateRouteAsync("Route", "Checkpoint A");
        var trip = await _client.CreateTripAsync(driverId, vehicleId, routeId, 500);
        await _client.StartTripAsync(trip.TripId);
        var request = new ReportIncidentRequest("", IncidentSeverity.Informational, null);

        // Act
        var response = await _http.PostAsJsonAsync($"/api/trips/{trip.TripId}/incidents", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReportIncident_WithTypeExceedingMaxLength_ShouldReturnBadRequest()
    {
        // Arrange
        var driverId = await _client.CreateDriverAsync("Driver", ResourceStatus.Available);
        var vehicleId = await _client.CreateVehicleAsync("Vehicle", "Type", 1000, ResourceStatus.Available);
        var routeId = await _client.CreateRouteAsync("Route", "Checkpoint A");
        var trip = await _client.CreateTripAsync(driverId, vehicleId, routeId, 500);
        await _client.StartTripAsync(trip.TripId);
        var longType = new string('a', 201); // MAX_NAME_LENGTH is 200
        var request = new ReportIncidentRequest(longType, IncidentSeverity.Informational, null);

        // Act
        var response = await _http.PostAsJsonAsync($"/api/trips/{trip.TripId}/incidents", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReportIncident_WithTypeAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var driverId = await _client.CreateDriverAsync("Driver", ResourceStatus.Available);
        var vehicleId = await _client.CreateVehicleAsync("Vehicle", "Type", 1000, ResourceStatus.Available);
        var routeId = await _client.CreateRouteAsync("Route", "Checkpoint A");
        var trip = await _client.CreateTripAsync(driverId, vehicleId, routeId, 500);
        await _client.StartTripAsync(trip.TripId);
        var maxLengthType = new string('a', 200); // MAX_NAME_LENGTH is 200
        var request = new ReportIncidentRequest(maxLengthType, IncidentSeverity.Informational, null);

        // Act
        var response = await _http.PostAsJsonAsync($"/api/trips/{trip.TripId}/incidents", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
