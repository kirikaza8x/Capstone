// using Events.Domain.DomainEvents;
// using Events.IntegrationEvents;
// using Shared.Application.Abstractions.EventBus;
// using Shared.Application.Abstractions.Messaging;

// namespace Events.Application.Events.EventHandlers
// {
//     internal sealed class EventCreatedEmbeddingDomainEventHandler(
//         IEventBus eventBus) : IDomainEventHandler<EventCreatedEmbeddingDomainEvent>
//     {
//         public async Task Handle(EventCreatedEmbeddingDomainEvent domainEvent, CancellationToken cancellationToken)
//         {
//             var integrationEvent = new EventCreatedEmbeddingIntegrationEvent(
//                 eventId: domainEvent.EventId,
//                 organizerId: domainEvent.OrganizerId,
//                 title: domainEvent.Title,
//                 description: domainEvent.Description,
//                 categories: domainEvent.Categories,
//                 hashtags: domainEvent.Hashtags,
//                 isActive: domainEvent.IsActive,
//                 createdAt: domainEvent.CreatedAt
//             );

//             await eventBus.PublishAsync(integrationEvent, cancellationToken);
//         }
//     }

// }
