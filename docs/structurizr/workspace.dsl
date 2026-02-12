workspace "Galactic Delivery System" "C4 model for The Great Galactic Delivery Race backend." {
  !adrs adrs

  model {
    dispatcher = person "Dispatcher" "Schedules trips by assigning drivers, vehicles, and routes."
    operationsManager = person "Operations Manager" "Oversees deliveries and operational performance."

    galacticDeliverySystem = softwareSystem "Galactic Delivery System" "Backend system supporting the Great Galactic Delivery Race." {
      tags "System"
    }

    // Core Domain (conceptual model)
    galacticRace = element "GalacticRace" "The Great Galactic Delivery Race operational context (multiple concurrent trips)." {
      tags "CoreDomain"
    }

    trip = element "Trip" "A delivery execution of a route by a specific driver and vehicle." {
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

    dispatcher -> galacticDeliverySystem "Creates and starts trips"
    operationsManager -> galacticDeliverySystem "Monitors trip progress and reviews summaries"

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

    container galacticDeliverySystem "Containers" "Container view (to be refined as containers are introduced)." {
      include *
      autolayout lr
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
    }
  }

  configuration {
    scope softwareSystem
  }
}
