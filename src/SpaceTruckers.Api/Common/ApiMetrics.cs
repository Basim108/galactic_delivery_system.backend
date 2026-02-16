using System.Diagnostics.Metrics;

namespace SpaceTruckers.Api.Common;

public static class ApiMetrics
{
    public const string METER_NAME = "SpaceTruckers.Api";
    public const string METER_VERSION = "1.0.0";

    private static readonly Meter Meter = new(METER_NAME, METER_VERSION);

    public static readonly Histogram<double> HttpEndpointDurationMs =
        Meter.CreateHistogram<double>(
            name: "http_endpoint_duration_ms",
            unit: "ms",
            description: "HTTP endpoint response time (ms) tagged by route/method/status_code.");
}
