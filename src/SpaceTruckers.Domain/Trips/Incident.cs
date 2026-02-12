namespace SpaceTruckers.Domain.Trips;

public enum IncidentSeverity
{
    Informational = 0,
    Warning = 1,
    Catastrophic = 2,
}

public sealed record Incident(string Type, IncidentSeverity Severity, string? Description);
