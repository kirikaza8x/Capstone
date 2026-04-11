using Events.Domain.DomainEvents;
using Events.IntegrationEvents;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.EventHandlers;

public sealed class EventChangedEmbeddingDomainEventHandler(
    IEventBus eventBus) : IDomainEventHandler<EventChangedEmbeddingDomainEvent>
{
    public async Task Handle(EventChangedEmbeddingDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var integrationEvent = new EventChangedEmbeddingIntegrationEvent(
            eventId: domainEvent.TargetEventId,
            organizerId: domainEvent.OrganizerId,
            title: domainEvent.Title,
            description: domainEvent.Description,
            categories: domainEvent.Categories,
            hashtags: domainEvent.Hashtags,
            isActive: domainEvent.IsActive,
            createdAt: domainEvent.CreatedAt,
            eventStartAt: domainEvent.EventStartAt
        );

        _ = Task.Run(() => eventBus.PublishAsync(integrationEvent, cancellationToken));
    }
}
