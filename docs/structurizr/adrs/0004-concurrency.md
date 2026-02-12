## Status
Accepted

## Context
The system must support multiple trips at the same time.
We must prevent:
- Double-booking Drivers or Vehicles across concurrent trips.
- Lost updates when multiple requests act on the same Trip (start/checkpoint/incident/complete).

We also need a strategy that works well with:
- Event-driven processing
- Potential future database persistence
- Horizontal scaling (multiple API instances)

## Decision
We will use **Optimistic Concurrency Control**.

- `Trip` uses a `Version` property that increments on each accepted state transition.
- Persistence updates require "expected version" matching the stored version.
- Resource booking (Driver/Vehicle reservations) also uses optimistic conflict detection to avoid double-booking.

## Why optimistic concurrency fits this domain
- Conflicts are expected to be relatively rare in normal operations.
- It avoids holding locks across network calls and long-running trip lifecycles.
- It maps directly to common persistence mechanisms (rowversion/ETag in SQL, document version in NoSQL).
- It works naturally with retry semantics and idempotency keys.

## Why not pessimistic locking
Pessimistic locking would require holding locks while a Trip is active (minutes/hours) or holding locks across distributed calls.
This causes:
- Reduced throughput under load.
- Higher risk of deadlocks/timeouts.
- Operational complexity when scaling out.

## Consequences
- Callers must handle conflicts (HTTP 409 / OptimisticConcurrencyException) and retry if appropriate.
- Implementation must ensure idempotency for retried commands (e.g., StartTrip).
- Future EF Core implementation will use concurrency tokens (e.g., rowversion) to preserve the same behavior.
