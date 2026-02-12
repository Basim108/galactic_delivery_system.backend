using FluentAssertions;
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

        var created = trip.RecordedEvents.OfType<TripCreated>().Should().ContainSingle().Subject;
        created.TripId.Should().Be(trip.Id);
    }

    [Fact]
    public void Start_ValidTrip_TransitionsToActiveAndIncrementsVersion()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = CreateTrip(cargoRequirement: 10, now);

        trip.Start(CargoCapacity.From(100), now);

        trip.Status.Should().Be(TripStatus.Active);
        trip.Version.Should().Be(1);
        trip.RecordedEvents.OfType<TripStarted>().Should().ContainSingle();
    }

    [Fact]
    public void Start_InsufficientCargoCapacity_ThrowsDomainRuleViolation()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = CreateTrip(cargoRequirement: 250, now);

        var act = () => trip.Start(CargoCapacity.From(100), now);

        act.Should().Throw<DomainRuleViolationException>()
            .Which.Code.Should().Be(DomainErrorCodes.INSUFFICIENT_CARGO_CAPACITY);
        trip.Status.Should().Be(TripStatus.Planned);
    }

    [Fact]
    public void Start_SameRequestIdTwice_DoesNotDuplicateTripStartedEvent()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = CreateTrip(cargoRequirement: 10, now);

        trip.Start(CargoCapacity.From(100), now, requestId: "REQ-1");
        trip.Start(CargoCapacity.From(100), now, requestId: "REQ-1");

        trip.RecordedEvents.OfType<TripStarted>().Should().ContainSingle();
    }

    [Fact]
    public void ReachCheckpoint_InOrder_UpdatesLastReachedCheckpoint()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = CreateTrip(cargoRequirement: 10, now, "Earth", "Luna Gate", "Mars Station");

        trip.Start(CargoCapacity.From(100), now);
        trip.ReachCheckpoint("Earth", now);
        trip.ReachCheckpoint("Luna Gate", now);

        trip.LastReachedCheckpointName.Should().Be("Luna Gate");
    }

    [Fact]
    public void ReachCheckpoint_OutOfOrder_ThrowsDomainRuleViolation()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = CreateTrip(cargoRequirement: 10, now, "Earth", "Luna Gate", "Mars Station");

        trip.Start(CargoCapacity.From(100), now);
        trip.ReachCheckpoint("Earth", now);

        var act = () => trip.ReachCheckpoint("Mars Station", now);

        act.Should().Throw<DomainRuleViolationException>()
            .Which.Code.Should().Be(DomainErrorCodes.CHECKPOINT_OUT_OF_ORDER);
    }

    [Fact]
    public void Complete_NotAtDestination_ThrowsDomainRuleViolation()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = CreateTrip(cargoRequirement: 10, now, "Earth", "Luna Gate", "Mars Station");

        trip.Start(CargoCapacity.From(100), now);
        trip.ReachCheckpoint("Earth", now);

        var act = () => trip.Complete(now);

        act.Should().Throw<DomainRuleViolationException>()
            .Which.Code.Should().Be(DomainErrorCodes.TRIP_NOT_AT_DESTINATION);
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

        trip.Status.Should().Be(TripStatus.Completed);
        trip.RecordedEvents.OfType<TripCompleted>().Should().ContainSingle();
    }

    [Fact]
    public void ReportIncident_Catastrophic_AbortsTrip()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = CreateTrip(cargoRequirement: 10, now);

        trip.Start(CargoCapacity.From(100), now);
        trip.ReportIncident(new Incident("CosmicStorm", IncidentSeverity.Catastrophic, null), now);

        trip.Status.Should().Be(TripStatus.Aborted);
        trip.RecordedEvents.OfType<TripAborted>().Should().ContainSingle();
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

        var act = () => trip.ReportIncident(new Incident("AsteroidField", IncidentSeverity.Catastrophic, null), now);

        act.Should().Throw<DomainRuleViolationException>()
            .Which.Code.Should().Be(DomainErrorCodes.TRIP_ALREADY_COMPLETED);
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
