using Events.Domain.DomainEvents;
using Events.IntegrationEvents;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.EventHandlers;

internal sealed class EventSuspendedDomainEventHandler(
    IEventBus eventBus) : IDomainEventHandler<EventSuspendedDomainEvent>
{
    public async Task Handle(EventSuspendedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var integrationEvent = new EventSuspendedIntegrationEvent(
            id: domainEvent.EventId,
            occurredOnUtc: domainEvent.OccurredOn,
            eventId: domainEvent.AggregateEventId,
            organizerId: domainEvent.OrganizerId,
            suspendedBy: domainEvent.SuspendedBy,
            eventTitle: domainEvent.EventTitle,
            suspensionReason: domainEvent.SuspensionReason,
            suspendedUntilAtUtc: domainEvent.SuspendedUntilAtUtc);

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
