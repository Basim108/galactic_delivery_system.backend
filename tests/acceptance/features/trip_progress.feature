Feature: Trip progress and completion summary
  In order to monitor deliveries
  As an Operations Manager
  I want trip events recorded and a summary produced when a trip completes

  Background:
    Given a Route "R-001" exists with ordered Checkpoints:
      | Sequence | Checkpoint |
      | 1        | Earth      |
      | 2        | Luna Gate  |
      | 3        | Mars Station |
    And Driver "D-001" is Available
    And Vehicle "V-001" is Available with CargoCapacity 1000
    And a Planned Trip "T-200" exists for Driver "D-001", Vehicle "V-001", Route "R-001"

  Scenario: Happy path - start trip, reach checkpoint, complete delivery
    When the Dispatcher starts Trip "T-200"
    Then Trip "T-200" should be in status "Active"
    And the system should record the Trip Event "TripStarted" for Trip "T-200"

    When the system records that Trip "T-200" reached Checkpoint "Luna Gate"
    Then the system should record the Trip Event "CheckpointReached" for Trip "T-200"
    And Trip "T-200" should have last reached Checkpoint "Luna Gate"

    When the system records that Trip "T-200" reached Checkpoint "Mars Station"
    Then the system should record the Trip Event "CheckpointReached" for Trip "T-200"
    And Trip "T-200" should have last reached Checkpoint "Mars Station"

    When the Dispatcher completes Trip "T-200"
    Then Trip "T-200" should be in status "Completed"
    And the system should record the Trip Event "TripCompleted" for Trip "T-200"
    And the system should produce a Trip Summary for Trip "T-200"

  Scenario: Successfully recording a checkpoint arrival
    Given Trip "T-200" is Active
    When the system records that Trip "T-200" reached Checkpoint "Mars Station"
    Then the system should record the Trip Event "CheckpointReached" for Trip "T-200"

  Scenario: Recording a catastrophic incident aborts the trip
    Given Trip "T-200" is Active
    When an Incident is reported for Trip "T-200" with severity "Catastrophic" and type "CosmicStorm"
    Then Trip "T-200" should be in status "Aborted"
    And the system should record the Trip Event "IncidentReported" for Trip "T-200"
    And the system should record the Trip Event "TripAborted" for Trip "T-200"

    When the Dispatcher attempts to complete Trip "T-200"
    Then the command should be rejected with reason "TripNotCompletable"
