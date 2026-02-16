# Development

This document describes local development workflows (tools, build, formatting, migrations, testing), plus developer-only features and diagrams.

## Restore tools and packages

```bash
dotnet tool restore
dotnet restore SpaceTruckers.sln
```

## Install Husky (Git hooks)

This repository uses **Husky.Net** as a .NET tool.

1. Restore tools (if needed):

   ```bash
   dotnet tool restore
   ```

2. Install the Git hooks:

   ```bash
   dotnet husky install
   ```

## How Husky pre-commit hooks work

- Hook scripts live in `.husky/`.
- The Git `pre-commit` hook is implemented in `.husky/pre-commit`.
- The current `pre-commit` hook runs `dotnet format` and then stages formatting changes (`git add -u`).

Where to add additional commands:

- For simple repo-specific commands, edit `.husky/pre-commit`.
- For a structured, cross-platform hook pipeline, add tasks to `.husky/task-runner.json` and call the Husky task runner from the hook script (for example, use `dotnet husky run --group pre-commit`).

## Formatting / linting

CI enforces formatting with `dotnet format`.

```bash
dotnet tool restore
dotnet restore SpaceTruckers.sln
dotnet format SpaceTruckers.sln --verify-no-changes --no-restore
```


## Build the project

```bash
dotnet build SpaceTruckers.sln -c Release
```

## EF Core migration operations

These examples use `dotnet tool run dotnet-ef ...`. If you have `dotnet-ef` installed globally, you can replace `dotnet tool run dotnet-ef` with `dotnet ef`.

### Set the connection string

When running migration commands, set the connection string via environment variable (recommended), user-secrets, or your shell profile. For example:

```bash
export ConnectionStrings__SpaceTruckersDb='Host=localhost;Port=5432;Database=spacetruckers;Username=spacetruckers;Password=spacetruckers'
```

### Add a new migration

```bash
dotnet tool run dotnet-ef migrations add <MigrationName> \
  --project src/SpaceTruckers.Infrastructure \
  --output-dir Persistence/EfCore/Migrations
```

### Remove the last migration

```bash
dotnet tool run dotnet-ef migrations remove \
  --project src/SpaceTruckers.Infrastructure
```

### Update the database

```bash
dotnet tool run dotnet-ef database update \
  --project src/SpaceTruckers.Infrastructure
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

## Feature flags on Development

Toggle `UseDomainPersistentStorage` in `src/SpaceTruckers.Api/appsettings.Development.json`:

```json
{
  "FeatureManagement": {
    "UseDomainPersistentStorage": true
  }
}
```

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

3. Export the token as an environment variable:

   ```bash
   export SONAR_TOKEN=<YOUR_LOCAL_TOKEN>
   ```

4. Install Java runtime 17.

   ```bash
   sudo apt update && sudo apt install openjdk-17-jre -y
   ```

5. Run analysis against the local SonarQube server:

   ```bash
   ./scripts/run-sonarqube-analysis.sh
   ```
