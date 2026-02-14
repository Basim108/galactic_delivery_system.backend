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
Restore tools and packages:

```bash
dotnet tool restore
dotnet restore SpaceTruckers.sln
```

Build:

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
