## Status
Accepted

## Context
We need a runnable system immediately to validate the domain model, CQRS handlers, concurrency rules, and acceptance criteria.
We also want the design to allow later adoption of a real database (e.g., Postgres via EF Core).

Constraints:
- Avoid premature infrastructure complexity.
- Keep the Domain pure.
- Keep the Application layer test-friendly.

## Decision
- The Application layer defines repository ports:
  - `ITripRepository`, `IDriverRepository`, `IVehicleRepository`, `IRouteRepository`
  - plus `IResourceBookingService` for double-booking prevention
- The Infrastructure layer provides thread-safe in-memory implementations using `ConcurrentDictionary`.

## Why in-memory now
- Fast feedback loop: domain rules can be exercised without provisioning a database.
- Deterministic tests: integration tests run quickly and locally.
- Focus on correctness of invariants, event ordering, and optimistic concurrency semantics.

## Why not EF Core yet
- Requires schema design, migrations, and database lifecycle management.
- Adds complexity unrelated to validating core domain rules.

## How we will switch to EF Core later
Because persistence is behind interfaces in the Application layer:
- Create an EF Core DbContext and repository implementations in Infrastructure.
- Preserve the same port contracts; application handlers do not change.
- Add transactional consistency and database-backed concurrency tokens.

## Consequences
- In-memory storage is not durable and resets on restart.
- Scalability is limited to a single process.
- This is acceptable for early development and specification validation.
