using System.Diagnostics.Metrics;

namespace SpaceTruckers.Application.Observability;

public static class SpaceTruckersMetrics
{
    public const string METER_NAME = "SpaceTruckers.Application";
    public const string METER_VERSION = "1.0.0";

    private static readonly Meter Meter = new(METER_NAME, METER_VERSION);

    public static readonly Counter<long> TripsProcessedTotal =
        Meter.CreateCounter<long>(
            name: "trips_processed_total",
            unit: "{trips}",
            description: "Total number of trips processed.");

    public static readonly Counter<long> IncidentsTotal =
        Meter.CreateCounter<long>(
            name: "incidents_total",
            unit: "{incidents}",
            description: "Total number of incidents reported.");

    public static readonly Counter<long> EventsProcessedTotal =
        Meter.CreateCounter<long>(
            name: "events_processed_total",
            unit: "{events}",
            description: "Total number of domain events processed.");
}
