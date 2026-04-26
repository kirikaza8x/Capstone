using Events.Domain.DomainEvents;
using Events.IntegrationEvents;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.EventHandlers;

public sealed class EventCancelledDomainEventHandler(
    IEventBus eventBus) : IDomainEventHandler<EventCancelledDomainEvent>
{
    public async Task Handle(EventCancelledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var integrationEvent = new EventCancelledIntegrationEvent(
            id: domainEvent.EventId,
            occurredOnUtc: domainEvent.OccurredOn,
            eventId: domainEvent.AggregateEventId,
            cancellationReason: domainEvent.CancellationReason);

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
