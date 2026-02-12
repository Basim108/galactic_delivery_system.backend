using FluentAssertions;
using NSubstitute;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Application.Trips.Commands;
using SpaceTruckers.Domain.Common;
using SpaceTruckers.Domain.Exceptions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Resources;
using SpaceTruckers.Domain.Routes;
using SpaceTruckers.Infrastructure.Persistence;

namespace SpaceTruckers.UnitTests.Application;

public sealed class CreateTripHandlerTests
{
    [Fact]
    public async Task Handle_DriverUnavailable_ThrowsDomainRuleViolation()
    {
        var driverRepo = new InMemoryDriverRepository();
        var vehicleRepo = new InMemoryVehicleRepository();
        var routeRepo = new InMemoryRouteRepository();
        var tripRepo = new InMemoryTripRepository();
        var booking = new InMemoryResourceBookingService();

        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(DateTimeOffset.UtcNow);

        var publisher = Substitute.For<IDomainEventPublisher>();

        var driverId = DriverId.New();
        var vehicleId = VehicleId.New();
        var routeId = RouteId.New();

        await driverRepo.AddAsync(Driver.Create(driverId, "D", ResourceStatus.Unavailable), CancellationToken.None);
        await vehicleRepo.AddAsync(Vehicle.Create(vehicleId, "V", "RocketVan", CargoCapacity.From(100), ResourceStatus.Available), CancellationToken.None);
        await routeRepo.AddAsync(Route.Create(routeId, "R", ["Earth", "Mars"]), CancellationToken.None);

        var handler = new CreateTripHandler(tripRepo, driverRepo, vehicleRepo, routeRepo, booking, clock, publisher);

        var act = async () => await handler.Handle(
            new CreateTripCommand(TripId.New(), driverId, vehicleId, routeId, CargoRequirement: 0),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainRuleViolationException>()
            .Where(ex => ex.Code == DomainErrorCodes.DRIVER_UNAVAILABLE);
    }
}
