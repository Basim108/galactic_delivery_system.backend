Feature: Trip creation
  In order to run the Great Galactic Delivery Race
  As a Dispatcher
  I want to create delivery trips by assigning drivers, vehicles, and routes

  Background:
    Given the following Drivers exist:
      | DriverId | Status     |
      | D-001    | Available  |
      | D-002    | Unavailable |
    And the following Vehicles exist:
      | VehicleId | Status     | CargoCapacity |
      | V-001     | Available  | 1000          |
      | V-002     | Unavailable | 500          |
    And the following Routes exist:
      | RouteId | Checkpoints                         |
      | R-001   | Earth -> Luna Gate -> Mars Station   |

  Scenario: Successful trip creation with valid resources
    When the Dispatcher creates a Trip with:
      | TripId | DriverId | VehicleId | RouteId |
      | T-100  | D-001    | V-001     | R-001   |
    Then the Trip "T-100" should be in status "Planned"
    And the Trip "T-100" should reference Driver "D-001"
    And the Trip "T-100" should reference Vehicle "V-001"
    And the Trip "T-100" should reference Route "R-001"
    And the system should record the Trip Event "TripCreated" for Trip "T-100"

  Scenario: Failure when assigning an unavailable Driver
    When the Dispatcher creates a Trip with:
      | TripId | DriverId | VehicleId | RouteId |
      | T-101  | D-002    | V-001     | R-001   |
    Then the command should be rejected with reason "DriverUnavailable"
    And the system should not create Trip "T-101"

  Scenario: Failure when assigning an unavailable Vehicle
    When the Dispatcher creates a Trip with:
      | TripId | DriverId | VehicleId | RouteId |
      | T-102  | D-001    | V-002     | R-001   |
    Then the command should be rejected with reason "VehicleUnavailable"
    And the system should not create Trip "T-102"
