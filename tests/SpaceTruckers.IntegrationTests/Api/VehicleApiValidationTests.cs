using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SpaceTruckers.Api.Contracts;
using SpaceTruckers.Domain.Resources;

namespace SpaceTruckers.IntegrationTests.Api;

public sealed class VehicleApiValidationTests : IClassFixture<SpaceTruckersApiFactory>
{
    private readonly HttpClient _http;

    public VehicleApiValidationTests(SpaceTruckersApiFactory factory)
    {
        _http = factory.CreateClient();
    }

    [Fact]
    public async Task CreateVehicle_WithValidData_ShouldSucceed()
    {
        // Arrange
        var request = new CreateVehicleRequest("Spaceship", "Cargo", 1000, ResourceStatus.Available);

        // Act
        var response = await _http.PostAsJsonAsync("/api/vehicles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CreateVehicleResponse>();
        body.Should().NotBeNull();
        body!.VehicleId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateVehicle_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateVehicleRequest("", "Cargo", 1000, ResourceStatus.Available);

        // Act
        var response = await _http.PostAsJsonAsync("/api/vehicles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateVehicle_WithNameExceedingMaxLength_ShouldReturnBadRequest()
    {
        // Arrange
        var longName = new string('a', 201); // MAX_NAME_LENGTH is 200
        var request = new CreateVehicleRequest(longName, "Cargo", 1000, ResourceStatus.Available);

        // Act
        var response = await _http.PostAsJsonAsync("/api/vehicles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateVehicle_WithEmptyType_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateVehicleRequest("Spaceship", "", 1000, ResourceStatus.Available);

        // Act
        var response = await _http.PostAsJsonAsync("/api/vehicles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateVehicle_WithTypeExceedingMaxLength_ShouldReturnBadRequest()
    {
        // Arrange
        var longType = new string('a', 101); // MAX_TYPE_LENGTH is 100
        var request = new CreateVehicleRequest("Spaceship", longType, 1000, ResourceStatus.Available);

        // Act
        var response = await _http.PostAsJsonAsync("/api/vehicles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateVehicle_WithTypeAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var maxLengthType = new string('a', 100); // MAX_TYPE_LENGTH is 100
        var request = new CreateVehicleRequest("Spaceship", maxLengthType, 1000, ResourceStatus.Available);

        // Act
        var response = await _http.PostAsJsonAsync("/api/vehicles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateVehicle_WithNegativeCargoCapacity_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateVehicleRequest("Spaceship", "Cargo", -1, ResourceStatus.Available);

        // Act
        var response = await _http.PostAsJsonAsync("/api/vehicles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateVehicle_WithZeroCargoCapacity_ShouldSucceed()
    {
        // Arrange
        var request = new CreateVehicleRequest("Spaceship", "Cargo", 0, ResourceStatus.Available);

        // Act
        var response = await _http.PostAsJsonAsync("/api/vehicles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
