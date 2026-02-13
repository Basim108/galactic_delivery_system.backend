using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SpaceTruckers.Api.Contracts;
using SpaceTruckers.Domain.Resources;

namespace SpaceTruckers.IntegrationTests.Api;

public sealed class DriverApiValidationTests : IClassFixture<SpaceTruckersApiFactory>
{
    private readonly HttpClient _http;

    public DriverApiValidationTests(SpaceTruckersApiFactory factory)
    {
        _http = factory.CreateClient();
    }

    [Fact]
    public async Task CreateDriver_WithValidName_ShouldSucceed()
    {
        // Arrange
        var request = new CreateDriverRequest("Valid Driver Name", ResourceStatus.Available);

        // Act
        var response = await _http.PostAsJsonAsync("/api/drivers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CreateDriverResponse>();
        body.Should().NotBeNull();
        body!.DriverId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateDriver_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateDriverRequest("", ResourceStatus.Available);

        // Act
        var response = await _http.PostAsJsonAsync("/api/drivers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateDriver_WithNameExceedingMaxLength_ShouldReturnBadRequest()
    {
        // Arrange
        var longName = new string('a', 201); // MAX_NAME_LENGTH is 200
        var request = new CreateDriverRequest(longName, ResourceStatus.Available);

        // Act
        var response = await _http.PostAsJsonAsync("/api/drivers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateDriver_WithNameAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var maxLengthName = new string('a', 200); // MAX_NAME_LENGTH is 200
        var request = new CreateDriverRequest(maxLengthName, ResourceStatus.Available);

        // Act
        var response = await _http.PostAsJsonAsync("/api/drivers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
