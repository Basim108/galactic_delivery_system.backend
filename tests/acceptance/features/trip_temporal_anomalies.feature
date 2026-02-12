Feature: Idempotency and temporal anomaly handling
  In order to remain reliable under retries and distributed event delivery
  As the system
  I want idempotent command handling and rejection of out-of-order events

  Background:
    Given Driver "D-030" is Available
    And Vehicle "V-030" is Available with CargoCapacity 1000
    And a Route "R-030" exists with ordered Checkpoints:
      | Sequence | Checkpoint |
      | 1        | Earth      |
      | 2        | Luna Gate  |
      | 3        | Mars Station |
    And a Planned Trip "T-500" exists for Driver "D-030", Vehicle "V-030", Route "R-030"

  Scenario: Idempotency - receiving StartTrip twice does not duplicate events
    When the Dispatcher starts Trip "T-500" with request id "REQ-1"
    Then Trip "T-500" should be in status "Active"
    And the system should record the Trip Event "TripStarted" exactly once for Trip "T-500"

    When the Dispatcher starts Trip "T-500" again with the same request id "REQ-1"
    Then the command should be treated as "IdempotentSuccess"
    And the system should record the Trip Event "TripStarted" exactly once for Trip "T-500"

  Scenario: Temporal anomaly - TripCompleted arrives before required checkpoints
    Given Trip "T-500" is Active
    And Trip "T-500" has last reached Checkpoint "Earth"
    When the Dispatcher completes Trip "T-500"
    Then the command should be rejected with reason "TripNotAtDestination"
    And Trip "T-500" should remain in status "Active"

  Scenario: Race condition - incident reported after trip completion
    Given Trip "T-500" is Completed
    When an Incident is reported for Trip "T-500" with severity "Catastrophic" and type "AsteroidField"
    Then the command should be rejected with reason "TripAlreadyCompleted"
    And the system should not change Trip "T-500" status from "Completed"
    And the system should not record the Trip Event "IncidentReported" for Trip "T-500"
