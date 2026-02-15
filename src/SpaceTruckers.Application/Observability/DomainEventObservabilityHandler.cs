using MediatR;
using Microsoft.Extensions.Logging;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Domain.Common;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.Application.Observability;

public sealed class DomainEventObservabilityHandler(ILogger<DomainEventObservabilityHandler> logger)
    : INotificationHandler<DomainEventNotification>
{
    public Task Handle(DomainEventNotification notification, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var domainEvent = notification.DomainEvent;
        var eventType = domainEvent.GetType().Name;

        SpaceTruckersMetrics.EventsProcessedTotal.Add(
            1,
            new KeyValuePair<string, object?>("event_type", eventType));

        switch (domainEvent)
        {
            case TripCreated e:
                {
                    using var scope = logger.BeginScope(new Dictionary<string, object?>
                    {
                        ["TripId"] = e.TripId.Value,
                        ["DriverId"] = e.DriverId.Value,
                        ["VehicleId"] = e.VehicleId.Value,
                        ["RouteId"] = e.RouteId.Value,
                    });

                    SpaceTruckersMetrics.TripsProcessedTotal.Add(1);
                    logger.LogInformation("Trip created");

                    break;
                }
            case TripStarted e:
                {
                    using var scope = logger.BeginScope(new Dictionary<string, object?> { ["TripId"] = e.TripId.Value });
                    logger.LogInformation(
                        "Trip status changed from {OldStatus} to {NewStatus}",
                        TripStatus.Planned,
                        TripStatus.Active);

                    break;
                }
            case CheckpointReached e:
                {
                    using var scope = logger.BeginScope(new Dictionary<string, object?> { ["TripId"] = e.TripId.Value });
                    logger.LogInformation(
                        "Trip reached checkpoint {CheckpointName} ({CheckpointId})",
                        e.CheckpointName,
                        e.CheckpointId.Value);

                    break;
                }
            case IncidentReported e:
                {
                    using var scope = logger.BeginScope(new Dictionary<string, object?> { ["TripId"] = e.TripId.Value });

                    SpaceTruckersMetrics.IncidentsTotal.Add(
                        1,
                        new KeyValuePair<string, object?>("incident_severity", e.Severity.ToString()));

                    logger.LogInformation(
                        "Incident occurred: {IncidentType} - {IncidentSeverity}",
                        e.Type,
                        e.Severity);

                    break;
                }
            case TripCompleted e:
                {
                    using var scope = logger.BeginScope(new Dictionary<string, object?> { ["TripId"] = e.TripId.Value });
                    logger.LogInformation(
                        "Trip status changed from {OldStatus} to {NewStatus}",
                        TripStatus.Active,
                        TripStatus.Completed);

                    break;
                }
            case TripAborted e:
                {
                    using var scope = logger.BeginScope(new Dictionary<string, object?> { ["TripId"] = e.TripId.Value });
                    logger.LogInformation(
                        "Trip status changed from {OldStatus} to {NewStatus}. Reason: {Reason}",
                        TripStatus.Active,
                        TripStatus.Aborted,
                        e.Reason);

                    break;
                }
            default:
                // Keep noise low: do not emit info logs for every unknown/high-frequency event.
                logger.LogDebug("Unhandled domain event type {EventType}", eventType);
                break;
        }

        return Task.CompletedTask;
    }
}
