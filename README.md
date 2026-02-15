# SpaceTruckers (Backend)

Developer guide for the **SpaceTruckers** backend (The Great Galactic Delivery Race).

## Repository structure

- `src/SpaceTruckers.Domain` - Domain model (entities, value objects, domain events, invariants)
- `src/SpaceTruckers.Application` - Use cases / application services / ports
- `src/SpaceTruckers.Infrastructure` - Adapters (persistence, messaging, external integrations)
- `src/SpaceTruckers.Api` - HTTP API (composition root)
- `tests/SpaceTruckers.UnitTests` - Unit tests (fast, isolated)
- `tests/SpaceTruckers.IntegrationTests` - Integration tests (API + infrastructure)
- `docs/domain` - Ubiquitous Language and domain documentation
- `docs/structurizr` - C4 model (Structurizr DSL)
- `docs/structurizr/adrs` - Architecture Decision Records (ADRs) rendered by Structurizr

## Prerequisites

- .NET SDK **10.x**
- Docker (for Structurizr Lite and local SonarQube)

## Getting started

### Restore tools and packages:

```bash
dotnet tool restore
dotnet restore SpaceTruckers.sln
```

### Build:

```bash
dotnet build SpaceTruckers.sln -c Release
```

## Features
### UseDomainPersistentStorage
Controls whether the application uses:
- EF Core repositories backed by PostgreSQL (enabled)
- In-memory repositories (disabled)

When enabled:
- The API stores domain entities such as trips, routes, drivers, etc in the postgres database.
- The API applies EF Core migrations automatically at startup.
- Repository implementations use PostgreSQL optimistic concurrency.

When disabled:
- The API runs without any database dependency.
- All data is process-local and is lost on restart.

## Feature flags
### Development
Toggle `UseDomainPersistentStorage` in `src/SpaceTruckers.Api/appsettings.Development.json`:

```json
{
  "FeatureManagement": {
    "UseDomainPersistentStorage": true
  }
}
```

### Staging / Production (Azure App Configuration)
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

## EF Core migrations (dotnet-ef)
This repo uses `dotnet-ef` as a local tool.

Install/restore the tool:

```bash
dotnet tool restore
```

Set a connection string (recommended: environment variable or user-secrets in Development):

```bash
export ConnectionStrings__SpaceTruckersDb='Host=localhost;Port=5432;Database=spacetruckers;Username=spacetruckers;Password=spacetruckers'
```

Add a migration:

```bash
dotnet tool run dotnet-ef migrations add <MigrationName> \
  --project src/SpaceTruckers.Infrastructure \
  --output-dir Persistence/EfCore/Migrations
```

Apply migrations:

```bash
dotnet tool run dotnet-ef database update \
  --project src/SpaceTruckers.Infrastructure
```

## Docker (API + PostgreSQL 18)
A local compose setup is provided:

```bash
docker compose up --build
```

- API: `http://localhost:8080`
- PostgreSQL: `localhost:5432` (service name is `db` inside the compose network)

## Formatting / linting

CI enforces formatting with `dotnet format`.

```bash
dotnet tool restore
dotnet restore SpaceTruckers.sln
dotnet format SpaceTruckers.sln --verify-no-changes --no-restore
```

## Testing (with coverage)

Test naming convention to follow when adding new tests:

- `MethodName_Condition_ExpectedResult()`

Run all tests with coverage (OpenCover):

```bash
dotnet tool restore
dotnet test SpaceTruckers.sln -c Release \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
```

Generate a human-readable coverage report:

```bash
dotnet reportgenerator \
  -reports:"**/TestResults/**/coverage.opencover.xml" \
  -targetdir:"./TestResults/coveragereport" \
  -reporttypes:"HtmlInline;Cobertura"
```

Open `TestResults/coveragereport/index.html` in a browser.

## Architecture diagrams (Structurizr)

Run Structurizr Lite locally:

```bash
./scripts/run-structurizr-lite.sh
```

Then open:

- `http://localhost:8080`

Validate the workspace DSL:

```bash
./scripts/validate-structurizr.sh
```

## SonarCloud (CI)

The GitHub Actions workflow expects:

- Repository secrets:
  - `SONAR_TOKEN`
  - `SONAR_ORG`
- Repository variable:
  - `SONAR_PROJECT_KEY`

## SonarQube locally (Docker)

1. Start SonarQube:

```bash
docker run --rm --name sonarqube \
  -p 9000:9000 \
  -e SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true \
  sonarqube:lts-community
```

2. Open `http://localhost:9000` and create a user token.
Go to `My Account > Security > Generate Token`.

ad export the token as an environment variable:
```bash
export SONAR_TOKEN=<YOUR_LOCAL_TOKEN>
```

3. Install java runtime 17

```sudo apt update && sudo apt install openjdk-17-jre -y```

4. Run analysis against the local SonarQube server:

```bash
./scripts/run-sonarqube-analysis.sh
```
