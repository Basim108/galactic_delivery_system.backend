using System.Diagnostics;
using Microsoft.AspNetCore.Routing;
using SpaceTruckers.Api.Common;

namespace SpaceTruckers.Api.Middleware;

public sealed class EndpointTimingMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var start = Stopwatch.GetTimestamp();

        try
        {
            await next(context);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(start);

            var route = GetRouteTemplate(context) ?? "unknown";
            var method = context.Request.Method;
            var statusCode = context.Response.StatusCode;

            ApiMetrics.HttpEndpointDurationMs.Record(
                elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("http_route", route),
                new KeyValuePair<string, object?>("http_method", method),
                new KeyValuePair<string, object?>("http_status_code", statusCode));
        }
    }

    private static string? GetRouteTemplate(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is null)
        {
            return null;
        }

        return endpoint is RouteEndpoint routeEndpoint
            ? routeEndpoint.RoutePattern.RawText
            : endpoint.DisplayName;
    }
}
