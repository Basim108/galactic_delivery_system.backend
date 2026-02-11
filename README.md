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

3. Run analysis against the local SonarQube server:

```bash
dotnet tool restore

SONAR_TOKEN="<YOUR_LOCAL_TOKEN>"
SONAR_PROJECT_KEY="SpaceTruckers"

dotnet sonarscanner begin \
  /k:"${SONAR_PROJECT_KEY}" \
  /d:sonar.host.url="http://localhost:9000" \
  /d:sonar.token="${SONAR_TOKEN}" \
  /d:sonar.cs.opencover.reportsPaths="**/TestResults/**/coverage.opencover.xml"

dotnet build SpaceTruckers.sln -c Release

dotnet test SpaceTruckers.sln -c Release \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

dotnet sonarscanner end /d:sonar.token="${SONAR_TOKEN}"
```
