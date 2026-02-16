# SpaceTruckers (Backend)

Developer guide for the **SpaceTruckers** backend (The Great Galactic Delivery Race).

- Task description: [`docs/task_description.md`](docs/task_description.md)
- Getting started: [`docs/getting_started.md`](docs/getting_started.md)
- Development: [`docs/development.md`](docs/development.md)
- DevOps: [`docs/devops.md`](docs/devops.md)

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

## Features

### UseDomainPersistentStorage

Controls whether the application uses:

- EF Core repositories backed by PostgreSQL (enabled)
- In-memory repositories (disabled)

When feature flag enabled:

- The API stores domain entities such as trips, routes, drivers, etc in the postgres database.
- The API applies EF Core migrations automatically at startup.
- Repository implementations use PostgreSQL optimistic concurrency.

When feature flag disabled:

- The API runs without any database dependency.
- All data is process-local and is lost on restart.
