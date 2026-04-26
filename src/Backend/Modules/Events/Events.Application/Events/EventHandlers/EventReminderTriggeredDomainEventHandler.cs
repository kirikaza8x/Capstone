using Events.Domain.DomainEvents;
using Events.IntegrationEvents;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.EventHandlers;

public sealed class EventReminderTriggeredDomainEventHandler(
    IEventBus eventBus) : IDomainEventHandler<EventReminderTriggeredDomainEvent>
{
    public async Task Handle(EventReminderTriggeredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var integrationEvent = new EventReminderTriggeredIntegrationEvent(
            id: domainEvent.EventId,
            occurredOnUtc: domainEvent.OccurredOn,
            eventId: domainEvent.AggregateEventId,
            organizerId: domainEvent.OrganizerId,
            eventTitle: domainEvent.EventTitle,
            eventStartAtUtc: domainEvent.EventStartAtUtc);

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
