using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SpaceTruckers.Api.Contracts;

namespace SpaceTruckers.IntegrationTests.Api;

public sealed class RouteApiValidationTests(SpaceTruckersApiFactory factory) : IClassFixture<SpaceTruckersApiFactory>
{
    private readonly HttpClient _http = factory.CreateClient();

    [Fact]
    public async Task CreateRoute_WithValidData_ShouldSucceed()
    {
        // Arrange
        var request = new CreateRouteRequest("Route 1", new[] { "Checkpoint A", "Checkpoint B" });

        // Act
        var response = await _http.PostAsJsonAsync("/api/routes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CreateRouteResponse>();
        body.Should().NotBeNull();
        body!.RouteId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateRoute_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRouteRequest("", new[] { "Checkpoint A" });

        // Act
        var response = await _http.PostAsJsonAsync("/api/routes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRoute_WithNameExceedingMaxLength_ShouldReturnBadRequest()
    {
        // Arrange
        var longName = new string('a', 201); // MAX_NAME_LENGTH is 200
        var request = new CreateRouteRequest(longName, new[] { "Checkpoint A" });

        // Act
        var response = await _http.PostAsJsonAsync("/api/routes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRoute_WithNullCheckpoints_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRouteRequest("Route 1", null!);

        // Act
        var response = await _http.PostAsJsonAsync("/api/routes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRoute_WithEmptyCheckpointsList_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRouteRequest("Route 1", Array.Empty<string>());

        // Act
        var response = await _http.PostAsJsonAsync("/api/routes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRoute_WithEmptyCheckpointName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRouteRequest("Route 1", new[] { "Checkpoint A", "" });

        // Act
        var response = await _http.PostAsJsonAsync("/api/routes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRoute_WithCheckpointNameExceedingMaxLength_ShouldReturnBadRequest()
    {
        // Arrange
        var longCheckpoint = new string('a', 201); // MAX_NAME_LENGTH is 200
        var request = new CreateRouteRequest("Route 1", new[] { "Checkpoint A", longCheckpoint });

        // Act
        var response = await _http.PostAsJsonAsync("/api/routes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRoute_WithCheckpointNameAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var maxLengthCheckpoint = new string('a', 200); // MAX_NAME_LENGTH is 200
        var request = new CreateRouteRequest("Route 1", new[] { maxLengthCheckpoint });

        // Act
        var response = await _http.PostAsJsonAsync("/api/routes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
