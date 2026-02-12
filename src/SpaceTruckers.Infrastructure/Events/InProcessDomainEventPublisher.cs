using MediatR;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Domain.Common;

namespace SpaceTruckers.Infrastructure.Events;

public sealed class InProcessDomainEventPublisher(IMediator mediator) : IDomainEventPublisher
{
    public async Task PublishAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(new DomainEventNotification(domainEvent), cancellationToken);
        }
    }
}
