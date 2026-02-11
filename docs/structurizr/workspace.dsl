workspace "Galactic Delivery System" "C4 model for The Great Galactic Delivery Race backend." {
  !adrs adrs

  model {
    operationsManager = person "Operations Manager" "Oversees deliveries and operational performance."

    galacticDeliverySystem = softwareSystem "Galactic Delivery System" "Backend system supporting the Great Galactic Delivery Race." {
      tags "System"
    }

    operationsManager -> galacticDeliverySystem "Monitors operations, manages deliveries"
  }

  views {
    systemContext galacticDeliverySystem "SystemContext" "System Context view." {
      include *
      autolayout lr
    }

    container galacticDeliverySystem "Containers" "Container view (to be refined as containers are introduced)." {
      include *
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
