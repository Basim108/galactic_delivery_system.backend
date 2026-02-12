## Status
Accepted

## Context
We are building the SpaceTruckers backend for "The Great Galactic Delivery Race".
The domain contains long-lived business concepts (Trip, Driver, Vehicle, Route, Incident) and must remain maintainable as requirements evolve.

We want to:
- Keep the Domain model independent from frameworks and infrastructure concerns.
- Make business rules testable with fast unit tests.
- Enable swapping infrastructure implementations (e.g., in-memory now, EF Core later).
- Avoid coupling application use-cases to transport (HTTP) or persistence.

## Decision
We will adopt **Clean Architecture (Onion Architecture)** with the following boundaries:
- **Domain** (`SpaceTruckers.Domain`): aggregates, value objects, domain events, invariants.
- **Application** (`SpaceTruckers.Application`): CQRS commands/queries and handlers, ports (interfaces) for persistence and event publishing.
- **Infrastructure** (`SpaceTruckers.Infrastructure`): adapters implementing Application ports (in-memory persistence, in-process event publisher).
- **API** (`SpaceTruckers.Api`): HTTP transport and composition root (DI wiring, validation, endpoint mapping).

Dependencies must point inward:
- API -> Application -> Domain
- Infrastructure -> Application + Domain

## Consequences
### Positive
- High testability: domain and handlers can be tested without HTTP or storage.
- Long-term maintainability: changes to infrastructure or transport do not require refactoring core business logic.
- Clear separation of concerns aligned with DDD boundaries.

### Negative
- More projects and abstractions compared to a single-layer API.
- Requires discipline to prevent boundary violations (e.g., infrastructure leaking into domain).

## Notes
This approach is preferred over "everything in the API project" because it creates explicit seams for:
- Concurrency policies
- Domain event dispatch
- Swapping persistence strategies
