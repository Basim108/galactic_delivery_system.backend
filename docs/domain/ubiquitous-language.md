## Purpose
This glossary defines the **Ubiquitous Language** for SpaceTruckers Inc. (The Great Galactic Delivery Race).
Terms are defined **strictly** to avoid ambiguity in code, tests, documentation, and diagrams.

## Core concepts (glossary)

### Driver
A **Driver** is a qualified pilot/operator who can be assigned to at most one **Active Trip** at a time.

- A Driver has an identity, qualifications, and an **Availability** status.
- A Driver is not the vehicle owner; the Driver is the human (or sentient) operator.

### Vehicle
A **Vehicle** is a physical craft used to transport cargo (e.g., HoverTruck, RocketVan, SpaceCycle).

- A Vehicle has a maximum **Cargo Capacity**.
- A Vehicle can be assigned to at most one **Active Trip** at a time.

### Cargo Capacity
**Cargo Capacity** is the maximum cargo mass/volume a Vehicle can safely transport.

- Capacity is a hard constraint: exceeding it is an invariant violation.
- Capacity must be treated as a domain value with explicit units (e.g., kg, m³) to avoid ambiguity.

### Route
A **Route** is a predefined journey plan from an **Origin** to a **Destination**, expressed as an ordered set of **Checkpoints**.

- A Route is static reference data for planning.
- A Route does not “move”; a Trip moves.

### Checkpoint
A **Checkpoint** is a named, ordered waypoint on a Route.

- A Route contains one or more Checkpoints.
- A Checkpoint can represent a planet, moon, station, or navigation gate.

### Trip
A **Trip** (also called a **Delivery Trip**) is the execution of a Route by a specific Driver and Vehicle at a specific time.

- A Trip is the central unit of tracking.
- A Trip has a lifecycle (see **Trip Status**).
- A Trip records an immutable sequence of **Trip Events**.

### Delivery
A **Delivery** is the business outcome: cargo is transported from the Route’s origin to its destination.

- In this domain, a Delivery is considered **Completed** when its Trip is Completed.

### Trip Event
A **Trip Event** is a time-stamped, immutable record of something that happened during a Trip.

Examples:
- TripCreated
- TripStarted
- CheckpointReached
- IncidentReported
- TripCompleted
- TripAborted

### Incident
An **Incident** is an “interesting” or exceptional Trip Event that represents an unexpected situation during a Trip.

- Incidents can be informational (e.g., MinorMaintenance) or severe.
- **Catastrophic Incident**: an incident that forces the Trip to be Aborted.

### GalacticRace
**GalacticRace** is the overarching operational context for “The Great Galactic Delivery Race”.

- The race may have rules/constraints (e.g., time windows, scoring) that influence Trips.
- The system must support multiple concurrent Trips that collectively represent the race activity.

### Dispatcher
A **Dispatcher** is an operational actor (user/system role) who schedules and starts Trips by assigning Drivers/Vehicles to Routes.

### Operations Manager
The **Operations Manager** is an operational actor who monitors Trip progress and reviews completion summaries.

## Trip status (state machine)
A Trip progresses through these statuses:

- Planned: Trip exists but has not started.
- Active: Trip has started and is in progress.
- Completed: Trip finished successfully.
- Aborted: Trip ended unsuccessfully due to a catastrophic incident or operational abort.

## Domain invariants (non-negotiable rules)

1. **Unique assignment (Driver)**
   - A Driver may be assigned to **at most one Active Trip**.

2. **Unique assignment (Vehicle)**
   - A Vehicle may be assigned to **at most one Active Trip**.

3. **Capacity constraint**
   - A Trip cannot start if the Vehicle’s Cargo Capacity is insufficient for the cargo requirement.

4. **Route consistency**
   - Checkpoints for a Trip must be reached in the same order as defined by the Route.

5. **Terminal status is terminal**
   - Once a Trip is Completed or Aborted, it cannot transition to another status.

## Concurrency & consistency definitions

### Active Trip
A Trip is **Active** from the moment TripStarted is accepted until the Trip is Completed or Aborted.

### Optimistic concurrency conflict
An **Optimistic Concurrency Conflict** occurs when two commands attempt to modify the same concurrency-protected resource (e.g., the same Driver or Vehicle availability) at the same logical version.

Expected outcome:
- One command succeeds.
- The other fails with an **Optimistic Concurrency Exception** (or an equivalent domain/application error).

## Idempotency & ordering rules

### Idempotent command handling
Commands that represent “facts” that may be delivered more than once (e.g., StartTrip) must be safe to retry.

- If the system receives the same StartTrip request twice for the same Trip, the second should be a **no-op** and return the already-started Trip state (no duplicate event).

### Temporal anomalies (out-of-order events)
Out-of-order events are events that contradict the Trip state machine or the Route order.

Expected handling:
- Reject out-of-order events with a conflict/invariant violation.
- Do not mutate Trip state when rejecting.

## Terms to avoid (ambiguous)

- “Journey” (use Route or Trip)
- “Stop” (use Checkpoint)
- “Problem” (use Incident with severity)
