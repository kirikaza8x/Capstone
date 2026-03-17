using Events.IntegrationEvents;
using Shared.Application.Abstractions.EventBus;

namespace Embedding.Application.EventHandlers
{
    internal sealed class EventCreatedEmbeddingIntegrationEventHandler 
        : IntegrationEventHandler<EventCreatedEmbeddingIntegrationEvent>
    {
        public override async Task Handle(EventCreatedEmbeddingIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            // Example: generate vector embedding
            // var vector = await _embeddingService.GenerateAsync(
            //     integrationEvent.Title,
            //     integrationEvent.Description,
            //     integrationEvent.Categories,
            //     integrationEvent.Hashtags);

            // Store in Qdrant
            // await _qdrantClient.StoreAsync(integrationEvent.EventId, vector, cancellationToken);
        }
    }


}
