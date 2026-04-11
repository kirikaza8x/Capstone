using Events.Domain.DomainEvents;
using Events.Domain.Repositories;
using Events.IntegrationEvents;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Messaging;

namespace Events.Application.EventMembers.EventHandlers;

public sealed class EventMemberInvitedDomainEventHandler(
    IEventRepository eventRepository,
    IEventBus eventBus) : IDomainEventHandler<EventMemberInvitedDomainEvent>
{
    public async Task Handle(EventMemberInvitedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(domainEvent.AggregateEventId, cancellationToken);

        if (@event is null)
            return;

        var integrationEvent = new EventMemberInvitedIntegrationEvent(
            id: Guid.NewGuid(),
            occurredOnUtc: DateTime.UtcNow,
            eventId: domainEvent.AggregateEventId,
            eventTitle: @event.Title,
            eventMemberId: domainEvent.EventMemberId,
            userId: domainEvent.UserId,
            email: domainEvent.Email);

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
