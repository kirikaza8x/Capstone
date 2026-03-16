using Events.Domain.DomainEvents;
using Events.IntegrationEvents;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.EventHandlers;

internal sealed class EventCompletedDomainEventHandler(
    IEventBus eventBus) : IDomainEventHandler<EventCompletedDomainEvent>
{
    public async Task Handle(EventCompletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var integrationEvent = new EventCompletedIntegrationEvent(
            id: domainEvent.EventId,
            occurredOnUtc: domainEvent.OccurredOn,
            eventId: domainEvent.EventId);

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}