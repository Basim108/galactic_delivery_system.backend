Feature: Trip invariants
  In order to ensure safe and valid deliveries
  As the system
  I want to enforce domain invariants before state transitions

  Background:
    Given Driver "D-020" is Available
    And Vehicle "V-020" is Available with CargoCapacity 100
    And a Route "R-020" exists with ordered Checkpoints:
      | Sequence | Checkpoint |
      | 1        | Earth      |
      | 2        | Mars Station |
    And a Planned Trip "T-400" exists for Driver "D-020", Vehicle "V-020", Route "R-020"

  Scenario: Attempting to start a trip with insufficient cargo capacity
    Given Trip "T-400" has CargoRequirement 250
    When the Dispatcher starts Trip "T-400"
    Then the command should be rejected with reason "InsufficientCargoCapacity"
    And Trip "T-400" should remain in status "Planned"
    And the system should not record the Trip Event "TripStarted" for Trip "T-400"

  Scenario: Attempting to reach a checkpoint out of order
    Given Trip "T-400" is Active
    And Trip "T-400" has last reached Checkpoint "Earth"
    When the system records that Trip "T-400" reached Checkpoint "Mars Station" with sequence number 2
    Then the system should record the Trip Event "CheckpointReached" for Trip "T-400"

    When the system records that Trip "T-400" reached Checkpoint "Earth" with sequence number 1
    Then the command should be rejected with reason "CheckpointOutOfOrder"
    And Trip "T-400" should have last reached Checkpoint "Mars Station"
