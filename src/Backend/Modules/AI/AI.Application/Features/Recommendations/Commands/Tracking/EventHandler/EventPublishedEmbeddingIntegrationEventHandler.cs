using Events.IntegrationEvents;
using Shared.Application.Abstractions.EventBus;

namespace Embedding.Application.EventHandlers
{
    

    internal sealed class EventPublishedEmbeddingIntegrationEventHandler 
        : IntegrationEventHandler<EventPublishedEmbeddingIntegrationEvent>
    {
        public override async Task Handle(EventPublishedEmbeddingIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            // Example: refresh embedding since event was published
            // var vector = await _embeddingService.GenerateAsync(
            //     integrationEvent.Title,
            //     integrationEvent.Description,
            //     integrationEvent.Categories,
            //     integrationEvent.Hashtags);

            // await _qdrantClient.UpdateAsync(integrationEvent.EventId, vector, cancellationToken);
        }
    }
}
