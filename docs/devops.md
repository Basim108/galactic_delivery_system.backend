# DevOps

This document covers operational concerns such as configuration in staging/production, observability, and CI scanning.

## Feature flags on Staging / Production (Azure App Configuration)

The API is configured to load feature flags from Azure App Configuration when `ASPNETCORE_ENVIRONMENT` is not `Development`.

Required configuration:

- Set `AzureAppConfiguration:ConnectionString` via environment variables or your platform configuration (do not commit it to the repo).
- Create a feature flag named `UseDomainPersistentStorage` in Azure App Configuration.

## Observability

This API uses native .NET structured logging + OpenTelemetry for metrics/tracing.

### Metrics

Custom metrics (via `System.Diagnostics.Metrics`):

- `http_endpoint_duration_ms` (histogram)
  - Dimensions:
    - `http_route`
    - `http_method`
    - `http_status_code`
  - Notes:
    - Explicit histogram buckets are configured so your metrics backend can calculate percentiles (P50/P95/P99).
- `trips_processed_total` (counter)
  - Incremented when a `TripCreated` domain event is published.
- `incidents_total` (counter)
  - Dimensions:
    - `incident_severity` (e.g. `Informational`, `Warning`, `Catastrophic`)
- `events_processed_total` (counter)
  - Dimensions:
    - `event_type` (domain event CLR type name)

Auto-instrumented HTTP server metrics are also enabled (OpenTelemetry ASP.NET Core instrumentation). Many backends will expose a standard `http.server.duration`-style metric tagged with method/route/status.

#### Percentiles (example)

Percentiles are typically computed in the backend from histogram buckets. For Prometheus-style querying, an example P95 query is:

```promql
histogram_quantile(0.95, sum(rate(http_endpoint_duration_ms_bucket[5m])) by (le, http_route, http_method))
```

### Logging

- Development: plain text console logs.
- Staging/Production: JSON console logs.
- Scopes are enabled (`IncludeScopes=true`) in all environments.
- Endpoints create scopes containing relevant domain identifiers (e.g. `TripId`, `DriverId`, etc.).
- MediatR handlers log trace messages:
  - `{HandlerName} starts`
  - `{HandlerName} ends`

Business events are logged from domain events (e.g. checkpoint reached, incident reported, trip status changes) using structured `LogInformation` messages.

## SonarCloud (CI)

The GitHub Actions workflow expects:

- Repository secrets:
  - `SONAR_TOKEN`
  - `SONAR_ORG`
- Repository variable:
  - `SONAR_PROJECT_KEY`
