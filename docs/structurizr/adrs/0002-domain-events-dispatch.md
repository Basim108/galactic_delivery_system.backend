## Status
Accepted

## Context
The SpaceTruckers domain is eventful: trips start, checkpoints are reached, incidents occur, trips complete or abort.
Operations requires reliable visibility and the system should be extendable (notifications, metrics, workflows).

We need a mechanism to:
- Capture domain-significant changes as **Domain Events** in the Domain layer.
- Dispatch those events after successful persistence.
- Keep the Domain model pure (no dependency on MediatR or messaging libraries).
- Allow future migration to persistent message brokers (Kafka, AWS SQS, etc.).

## Decision
- Domain events are represented as plain C# records in `SpaceTruckers.Domain` (no framework interfaces).
- The Application layer defines a port: `IDomainEventPublisher`.
- The Infrastructure layer provides an in-process implementation: `InProcessDomainEventPublisher`, using **MediatR** to publish `DomainEventNotification`.

## Why MediatR (now)
- Simple in-process pub/sub with DI integration.
- Encourages small, decoupled handlers (notifications can be added without changing aggregates).
- Keeps commands/queries (CQRS) and notifications in one mental model.

## Why not .NET Channels (for this requirement)
Channels are a great low-level primitive, but:
- They require us to design message routing, subscription, and handler lifetimes ourselves.
- They do not provide a natural separation between command handling vs domain event handling out-of-the-box.
- They are not a drop-in integration point for external brokers.

## How this enables external brokers later
Because Application depends on `IDomainEventPublisher` (an interface), we can replace the infrastructure implementation with:
- An outbox + broker publisher (Kafka/SQS/etc.)
- A durable event store
- A hybrid approach (in-process for local concerns, broker for cross-service concerns)

The Domain layer remains unchanged.

## Consequences
- Domain events are first-class and testable.
- Event dispatch is pluggable.
- In-process dispatch does not guarantee durability; durability will be added with a broker/outbox when needed.
