// using Events.Domain.DomainEvents;
// using Events.IntegrationEvents;
// using Shared.Application.Abstractions.EventBus;
// using Shared.Application.Abstractions.Messaging;

// namespace Events.Application.Events.EventHandlers
// {
    
//     internal sealed class EventPublishedEmbeddingDomainEventHandler(
//         IEventBus eventBus) : IDomainEventHandler<EventPublishedEmbeddingDomainEvent>
//     {
//         public async Task Handle(EventPublishedEmbeddingDomainEvent domainEvent, CancellationToken cancellationToken)
//         {
//             var integrationEvent = new EventPublishedEmbeddingIntegrationEvent(
//                 eventId: domainEvent.EventId,
//                 organizerId: domainEvent.OrganizerId,
//                 title: domainEvent.Title,
//                 description: domainEvent.Description,
//                 categories: domainEvent.Categories,
//                 hashtags: domainEvent.Hashtags,
//                 isActive: domainEvent.IsActive,
//                 updatedAt: domainEvent.UpdatedAt
//             );

//             await eventBus.PublishAsync(integrationEvent, cancellationToken);
//         }
//     }
// }
