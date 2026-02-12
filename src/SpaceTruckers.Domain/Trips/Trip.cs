using SpaceTruckers.Domain.Common;
using SpaceTruckers.Domain.Exceptions;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Resources;

namespace SpaceTruckers.Domain.Trips;

public sealed class Trip
{
    private readonly List<IDomainEvent> _recordedEvents = new();
    private readonly List<IDomainEvent> _uncommittedEvents = new();
    private readonly HashSet<string> _startTripRequestIds = new(StringComparer.Ordinal);

    private readonly IReadOnlyList<TripCheckpoint> _checkpoints;

    private Trip(
        TripId id,
        DriverId driverId,
        VehicleId vehicleId,
        RouteId routeId,
        CargoRequirement cargoRequirement,
        IReadOnlyList<TripCheckpoint> checkpoints)
    {
        if (checkpoints.Count == 0)
        {
            throw new ArgumentException("Trip must have at least one checkpoint.", nameof(checkpoints));
        }

        Id = id;
        DriverId = driverId;
        VehicleId = vehicleId;
        RouteId = routeId;
        CargoRequirement = cargoRequirement;
        _checkpoints = checkpoints.ToArray();

        Status = TripStatus.Planned;
        Version = 0;
        LastReachedCheckpointIndex = -1;
    }

    public TripId Id { get; }
    public DriverId DriverId { get; }
    public VehicleId VehicleId { get; }
    public RouteId RouteId { get; }

    public CargoRequirement CargoRequirement { get; private set; }

    public TripStatus Status { get; private set; }

    /// <summary>
    /// Optimistic concurrency token.
    /// Incremented on each accepted state transition.
    /// </summary>
    public int Version { get; private set; }

    public int LastReachedCheckpointIndex { get; private set; }

    public string? LastReachedCheckpointName =>
        LastReachedCheckpointIndex >= 0 && LastReachedCheckpointIndex < _checkpoints.Count
            ? _checkpoints[LastReachedCheckpointIndex].Name
            : null;

    public IReadOnlyList<TripCheckpoint> Checkpoints => _checkpoints;

    public IReadOnlyList<IDomainEvent> RecordedEvents => _recordedEvents;

    public static Trip Create(
        TripId id,
        DriverId driverId,
        VehicleId vehicleId,
        RouteId routeId,
        CargoRequirement cargoRequirement,
        IReadOnlyList<TripCheckpoint> checkpoints,
        DateTimeOffset now)
    {
        var trip = new Trip(id, driverId, vehicleId, routeId, cargoRequirement, checkpoints);
        trip.Record(new TripCreated(trip.Id, trip.DriverId, trip.VehicleId, trip.RouteId, now));
        return trip;
    }

    public void Start(CargoCapacity vehicleCargoCapacity, DateTimeOffset now, string? requestId = null)
    {
        if (requestId is not null && _startTripRequestIds.Contains(requestId))
        {
            return;
        }

        if (Status == TripStatus.Active)
        {
            throw new DomainRuleViolationException(DomainErrorCodes.TRIP_ALREADY_STARTED, "Trip is already active.");
        }

        if (Status == TripStatus.Completed)
        {
            throw new DomainRuleViolationException(DomainErrorCodes.TRIP_ALREADY_COMPLETED, "Trip is already completed.");
        }

        if (Status == TripStatus.Aborted)
        {
            throw new DomainRuleViolationException(DomainErrorCodes.TRIP_ALREADY_ABORTED, "Trip is already aborted.");
        }

        if (vehicleCargoCapacity.Value < CargoRequirement.Value)
        {
            throw new DomainRuleViolationException(
                DomainErrorCodes.INSUFFICIENT_CARGO_CAPACITY,
                $"Vehicle capacity {vehicleCargoCapacity.Value} is insufficient for cargo requirement {CargoRequirement.Value}.");
        }

        Status = TripStatus.Active;
        IncrementVersion();
        Record(new TripStarted(Id, now));

        // Starting a trip implies the vehicle is at the route origin (first checkpoint).
        if (LastReachedCheckpointIndex < 0)
        {
            LastReachedCheckpointIndex = 0;
            var origin = _checkpoints[0];
            Record(new CheckpointReached(Id, origin.Id, origin.Name, origin.Sequence, now));
        }

        if (requestId is not null)
        {
            _startTripRequestIds.Add(requestId);
        }
    }

    public void ReachCheckpoint(string checkpointName, DateTimeOffset now)
    {
        EnsureActive();

        var index = FindCheckpointIndex(checkpointName);
        var expectedIndex = LastReachedCheckpointIndex + 1;

        if (index < expectedIndex)
        {
            // Duplicate delivery of the same event is treated as idempotent.
            if (index == LastReachedCheckpointIndex)
            {
                return;
            }

            throw new DomainRuleViolationException(
                DomainErrorCodes.CHECKPOINT_OUT_OF_ORDER,
                $"Checkpoint '{checkpointName}' cannot be reached after checkpoint '{LastReachedCheckpointName}'.");
        }

        if (index > expectedIndex)
        {
            throw new DomainRuleViolationException(
                DomainErrorCodes.CHECKPOINT_OUT_OF_ORDER,
                $"Expected checkpoint '{_checkpoints[expectedIndex].Name}' but got '{checkpointName}'.");
        }

        LastReachedCheckpointIndex = index;
        IncrementVersion();

        var checkpoint = _checkpoints[index];
        Record(new CheckpointReached(Id, checkpoint.Id, checkpoint.Name, checkpoint.Sequence, now));
    }

    public void ReportIncident(Incident incident, DateTimeOffset now)
    {
        if (Status == TripStatus.Completed)
        {
            throw new DomainRuleViolationException(DomainErrorCodes.TRIP_ALREADY_COMPLETED, "Cannot report incidents for a completed trip.");
        }

        if (Status == TripStatus.Aborted)
        {
            throw new DomainRuleViolationException(DomainErrorCodes.TRIP_ALREADY_ABORTED, "Cannot report incidents for an aborted trip.");
        }

        EnsureActive();

        IncrementVersion();
        Record(new IncidentReported(Id, incident.Type, incident.Severity, incident.Description, now));

        if (incident.Severity == IncidentSeverity.Catastrophic)
        {
            Status = TripStatus.Aborted;
            IncrementVersion();
            Record(new TripAborted(Id, $"Catastrophic incident: {incident.Type}", now));
        }
    }

    public void Complete(DateTimeOffset now)
    {
        if (Status != TripStatus.Active)
        {
            throw new DomainRuleViolationException(DomainErrorCodes.TRIP_NOT_COMPLETABLE, "Trip is not in a completable state.");
        }

        if (LastReachedCheckpointIndex != _checkpoints.Count - 1)
        {
            throw new DomainRuleViolationException(
                DomainErrorCodes.TRIP_NOT_AT_DESTINATION,
                "Trip cannot be completed before reaching the destination checkpoint.");
        }

        Status = TripStatus.Completed;
        IncrementVersion();
        Record(new TripCompleted(Id, now));
    }

    public IReadOnlyCollection<IDomainEvent> DequeueUncommittedEvents()
    {
        var events = _uncommittedEvents.ToArray();
        _uncommittedEvents.Clear();
        return events;
    }

    public Trip CloneForPersistence()
    {
        var clone = new Trip(Id, DriverId, VehicleId, RouteId, CargoRequirement, _checkpoints)
        {
            Status = Status,
            Version = Version,
            LastReachedCheckpointIndex = LastReachedCheckpointIndex,
        };

        foreach (var requestId in _startTripRequestIds)
        {
            clone._startTripRequestIds.Add(requestId);
        }

        clone._recordedEvents.AddRange(_recordedEvents);
        // Do not persist uncommitted events; they are transient.

        return clone;
    }

    private void EnsureActive()
    {
        if (Status != TripStatus.Active)
        {
            throw new DomainRuleViolationException(DomainErrorCodes.TRIP_NOT_ACTIVE, "Trip must be active.");
        }
    }

    private int FindCheckpointIndex(string checkpointName)
    {
        for (var i = 0; i < _checkpoints.Count; i++)
        {
            if (string.Equals(_checkpoints[i].Name, checkpointName, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        throw new DomainRuleViolationException(
            DomainErrorCodes.CHECKPOINT_OUT_OF_ORDER,
            $"Checkpoint '{checkpointName}' does not exist on this route.");
    }

    private void Record(IDomainEvent domainEvent)
    {
        _recordedEvents.Add(domainEvent);
        _uncommittedEvents.Add(domainEvent);
    }

    private void IncrementVersion() => Version++;
}
