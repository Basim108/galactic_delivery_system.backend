using SpaceTruckers.Domain.Common;
using SpaceTruckers.Domain.Exceptions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Resources;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.UnitTests.Trips;

public sealed class TripTests
{
    [Fact]
    public void Create_ValidInput_EmitsTripCreatedEvent()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = Trip.Create(
            TripId.New(),
            DriverId.New(),
            VehicleId.New(),
            RouteId.New(),
            CargoRequirement.From(10),
            CreateCheckpoints("Earth", "Mars"),
            now);

        var created = Assert.Single(trip.RecordedEvents.OfType<TripCreated>());
        Assert.Equal(trip.Id, created.TripId);
    }

    [Fact]
    public void Start_ValidTrip_TransitionsToActiveAndIncrementsVersion()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = CreateTrip(cargoRequirement: 10, now);

        trip.Start(CargoCapacity.From(100), now);

        Assert.Equal(TripStatus.Active, trip.Status);
        Assert.Equal(1, trip.Version);
        Assert.Single(trip.RecordedEvents.OfType<TripStarted>());
    }

    [Fact]
    public void Start_InsufficientCargoCapacity_ThrowsDomainRuleViolation()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = CreateTrip(cargoRequirement: 250, now);

        var ex = Assert.Throws<DomainRuleViolationException>(() => trip.Start(CargoCapacity.From(100), now));
        Assert.Equal(DomainErrorCodes.INSUFFICIENT_CARGO_CAPACITY, ex.Code);
        Assert.Equal(TripStatus.Planned, trip.Status);
    }

    [Fact]
    public void Start_SameRequestIdTwice_DoesNotDuplicateTripStartedEvent()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = CreateTrip(cargoRequirement: 10, now);

        trip.Start(CargoCapacity.From(100), now, requestId: "REQ-1");
        trip.Start(CargoCapacity.From(100), now, requestId: "REQ-1");

        Assert.Single(trip.RecordedEvents.OfType<TripStarted>());
    }

    [Fact]
    public void ReachCheckpoint_InOrder_UpdatesLastReachedCheckpoint()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = CreateTrip(cargoRequirement: 10, now, "Earth", "Luna Gate", "Mars Station");

        trip.Start(CargoCapacity.From(100), now);
        trip.ReachCheckpoint("Earth", now);
        trip.ReachCheckpoint("Luna Gate", now);

        Assert.Equal("Luna Gate", trip.LastReachedCheckpointName);
    }

    [Fact]
    public void ReachCheckpoint_OutOfOrder_ThrowsDomainRuleViolation()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = CreateTrip(cargoRequirement: 10, now, "Earth", "Luna Gate", "Mars Station");

        trip.Start(CargoCapacity.From(100), now);
        trip.ReachCheckpoint("Earth", now);

        var ex = Assert.Throws<DomainRuleViolationException>(() => trip.ReachCheckpoint("Mars Station", now));
        Assert.Equal(DomainErrorCodes.CHECKPOINT_OUT_OF_ORDER, ex.Code);
    }

    [Fact]
    public void Complete_NotAtDestination_ThrowsDomainRuleViolation()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = CreateTrip(cargoRequirement: 10, now, "Earth", "Luna Gate", "Mars Station");

        trip.Start(CargoCapacity.From(100), now);
        trip.ReachCheckpoint("Earth", now);

        var ex = Assert.Throws<DomainRuleViolationException>(() => trip.Complete(now));
        Assert.Equal(DomainErrorCodes.TRIP_NOT_AT_DESTINATION, ex.Code);
    }

    [Fact]
    public void Complete_AtDestination_TransitionsToCompleted()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = CreateTrip(cargoRequirement: 10, now, "Earth", "Luna Gate", "Mars Station");

        trip.Start(CargoCapacity.From(100), now);
        trip.ReachCheckpoint("Earth", now);
        trip.ReachCheckpoint("Luna Gate", now);
        trip.ReachCheckpoint("Mars Station", now);

        trip.Complete(now);

        Assert.Equal(TripStatus.Completed, trip.Status);
        Assert.Single(trip.RecordedEvents.OfType<TripCompleted>());
    }

    [Fact]
    public void ReportIncident_Catastrophic_AbortsTrip()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = CreateTrip(cargoRequirement: 10, now);

        trip.Start(CargoCapacity.From(100), now);
        trip.ReportIncident(new Incident("CosmicStorm", IncidentSeverity.Catastrophic, null), now);

        Assert.Equal(TripStatus.Aborted, trip.Status);
        Assert.Single(trip.RecordedEvents.OfType<TripAborted>());
    }

    [Fact]
    public void ReportIncident_OnCompletedTrip_ThrowsDomainRuleViolation()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = CreateTrip(cargoRequirement: 10, now, "Earth", "Mars");

        trip.Start(CargoCapacity.From(100), now);
        trip.ReachCheckpoint("Earth", now);
        trip.ReachCheckpoint("Mars", now);
        trip.Complete(now);

        var ex = Assert.Throws<DomainRuleViolationException>(() =>
            trip.ReportIncident(new Incident("AsteroidField", IncidentSeverity.Catastrophic, null), now));

        Assert.Equal(DomainErrorCodes.TRIP_ALREADY_COMPLETED, ex.Code);
    }

    private static Trip CreateTrip(int cargoRequirement, DateTimeOffset now, params string[] checkpoints) =>
        Trip.Create(
            TripId.New(),
            DriverId.New(),
            VehicleId.New(),
            RouteId.New(),
            CargoRequirement.From(cargoRequirement),
            CreateCheckpoints(checkpoints.Length == 0 ? ["Earth", "Mars"] : checkpoints),
            now);

    private static IReadOnlyList<TripCheckpoint> CreateCheckpoints(params string[] checkpointNames)
    {
        var list = new List<TripCheckpoint>(checkpointNames.Length);
        for (var i = 0; i < checkpointNames.Length; i++)
        {
            list.Add(new TripCheckpoint(CheckpointId.New(), i + 1, checkpointNames[i]));
        }

        return list;
    }
}
