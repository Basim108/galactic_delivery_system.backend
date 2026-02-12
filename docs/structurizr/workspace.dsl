workspace "Galactic Delivery System" "C4 model for The Great Galactic Delivery Race backend." {
  !adrs adrs

  model {
    dispatcher = person "Dispatcher" "Schedules trips by assigning drivers, vehicles, and routes."
    operationsManager = person "Operations Manager" "Oversees deliveries and operational performance."

    galacticDeliverySystem = softwareSystem "Galactic Delivery System" "Backend system supporting the Great Galactic Delivery Race." {
      tags "System"

      api = container "SpaceTruckers API" "ASP.NET Core Minimal API (SpaceTruckers.Api)." "C#/.NET" {
        tags "Container"
      }

      application = container "Application Layer" "CQRS command/query handlers using MediatR (SpaceTruckers.Application)." "C#/.NET" {
        tags "Container"

        tripCommandHandlers = component "TripCommandHandlers" "MediatR IRequestHandlers for Trip commands." "C#"
        tripRepository = component "ITripRepository" "Trip persistence port (InMemory now, EF Core later)." "C#"
        domainEventPublisher = component "IDomainEventPublisher" "Publishes domain events (in-process now, persistent broker later)." "C#"
      }

      infrastructure = container "Infrastructure" "In-memory persistence and event dispatch adapters (SpaceTruckers.Infrastructure)." "C#/.NET" {
        tags "Container"
      }
    }

    // Core Domain (conceptual model)
    galacticRace = element "GalacticRace" "The Great Galactic Delivery Race operational context (multiple concurrent trips)." {
      tags "CoreDomain"
    }

    trip = element "Trip" "Aggregate root: tracks lifecycle and events; optimistic concurrency via Version." {
      tags "CoreDomain"
    }

    driver = element "Driver" "A qualified pilot/operator assigned to trips." {
      tags "CoreDomain"
    }

    vehicle = element "Vehicle" "A craft with cargo capacity used to transport cargo." {
      tags "CoreDomain"
    }

    route = element "Route" "An ordered set of checkpoints from origin to destination." {
      tags "CoreDomain"
    }

    checkpoint = element "Checkpoint" "An ordered waypoint on a route." {
      tags "CoreDomain"
    }

    incident = element "Incident" "An unexpected event during a trip (may be catastrophic)." {
      tags "CoreDomain"
    }

    // System interactions
    dispatcher -> api "Creates and starts trips" "HTTPS"
    operationsManager -> api "Monitors trip progress and reviews summaries" "HTTPS"

    api -> application "Sends commands/queries" "MediatR"
    api -> tripCommandHandlers "Dispatches commands" "MediatR"

    tripCommandHandlers -> tripRepository "Load/Save Trips" "ITripRepository"
    tripRepository -> trip "Loads/Saves" "InMemory"
    tripCommandHandlers -> trip "Executes domain logic" "In-process"
    tripCommandHandlers -> domainEventPublisher "Publishes" "IDomainEventPublisher"

    application -> infrastructure "Uses" "InMemory repositories"

    // Conceptual domain relationships
    galacticDeliverySystem -> galacticRace "Runs"

    galacticRace -> trip "Includes" "1..*"
    trip -> driver "Assigns"
    trip -> vehicle "Assigns"
    trip -> route "Follows"
    route -> checkpoint "Defines (ordered)" "1..*"
    trip -> incident "Records" "0..*"
  }

  views {
    systemContext galacticDeliverySystem "SystemContext" "System Context view." {
      include dispatcher
      include operationsManager
      include galacticDeliverySystem
      autolayout lr
    }

    container galacticDeliverySystem "Containers" "Container view." {
      include dispatcher
      include operationsManager
      include api
      include application
      include infrastructure
      autolayout lr
    }

    component application "TripComponents" "How Trip command handlers use repository and event publishing ports." {
      include api
      include trip

      include tripCommandHandlers
      include tripRepository
      include domainEventPublisher

      autolayout lr
    }

    dynamic application "StartTripFlow" "Dynamic view - Start Trip command." {
      dispatcher -> api "POST /api/trips/{id}/start"
      api -> tripCommandHandlers "StartTripCommand"
      tripCommandHandlers -> tripRepository "GetAsync(TripId)"
      tripRepository -> trip "Load Trip"
      tripCommandHandlers -> trip "Start()"
      tripCommandHandlers -> tripRepository "UpdateAsync(trip, expectedVersion)"
      tripCommandHandlers -> domainEventPublisher "Publish(domainEvents)"
    }

    custom "DomainModel" "Core domain conceptual model." {
      include galacticRace
      include trip
      include driver
      include vehicle
      include route
      include checkpoint
      include incident
      autolayout lr
    }

    styles {
      element "Person" {
        shape person
        background #08427b
        color #ffffff
      }

      element "Software System" {
        background #1168bd
        color #ffffff
      }

      element "Container" {
        background #438dd5
        color #ffffff
      }

      element "CoreDomain" {
        background #2d6a4f
        color #ffffff
      }
    }
  }

  configuration {
    scope softwareSystem
  }
}
