Feature: Concurrency controls for resource assignment
  In order to prevent double-booking
  As the system
  I want optimistic concurrency checks when assigning Drivers and Vehicles

  Background:
    Given Driver "D-010" is Available
    And Vehicle "V-010" is Available with CargoCapacity 1000
    And Routes "R-010" and "R-011" exist

  Scenario: Two dispatchers try to assign the same Driver to different trips simultaneously
    Given Dispatcher "Alice" is creating Trip "T-300" with Driver "D-010", Vehicle "V-010", Route "R-010"
    And Dispatcher "Bob" is creating Trip "T-301" with Driver "D-010", Vehicle "V-010", Route "R-011"

    When Dispatcher "Alice" and Dispatcher "Bob" submit their create-trip commands concurrently
    Then exactly one of the trips "T-300" or "T-301" should be created successfully
    And the other command should fail with an "Optimistic Concurrency Exception"
    And Driver "D-010" should be assigned to exactly one Active Trip

  Scenario: Two dispatchers try to assign the same Vehicle to different trips simultaneously
    Given Driver "D-011" is Available
    And Driver "D-012" is Available
    And Dispatcher "Alice" is creating Trip "T-302" with Driver "D-011", Vehicle "V-010", Route "R-010"
    And Dispatcher "Bob" is creating Trip "T-303" with Driver "D-012", Vehicle "V-010", Route "R-011"

    When Dispatcher "Alice" and Dispatcher "Bob" submit their create-trip commands concurrently
    Then exactly one of the trips "T-302" or "T-303" should be created successfully
    And the other command should fail with an "Optimistic Concurrency Exception"
    And Vehicle "V-010" should be assigned to exactly one Active Trip
