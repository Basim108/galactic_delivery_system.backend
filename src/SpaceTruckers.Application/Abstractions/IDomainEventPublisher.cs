using MediatR;
using SpaceTruckers.Domain.Common;

namespace SpaceTruckers.Application.Abstractions;

public interface IDomainEventPublisher
{
    Task PublishAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}

public sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
