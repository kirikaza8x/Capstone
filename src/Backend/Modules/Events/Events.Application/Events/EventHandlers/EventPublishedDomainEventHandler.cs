using Events.Domain.DomainEvents;
using Events.IntegrationEvents;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.EventHandlers;

internal sealed class EventPublishedDomainEventHandler(
    IEventBus eventBus) : IDomainEventHandler<EventPublishedDomainEvent>
{
    public async Task Handle(EventPublishedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var integrationEvent = new EventPublishedIntegrationEvent(
            id: domainEvent.EventId,
            occurredOnUtc: domainEvent.OccurredOn,
            aggregateId: domainEvent.AggregateId);

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}