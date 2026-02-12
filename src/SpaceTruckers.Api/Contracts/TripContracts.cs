using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.Api.Contracts;

public sealed record CreateTripRequest(Guid DriverId, Guid VehicleId, Guid RouteId, int CargoRequirement, Guid? TripId);

public sealed record StartTripRequest(string? RequestId);

public sealed record ReachCheckpointRequest(string CheckpointName);

public sealed record ReportIncidentRequest(string Type, IncidentSeverity Severity, string? Description);
