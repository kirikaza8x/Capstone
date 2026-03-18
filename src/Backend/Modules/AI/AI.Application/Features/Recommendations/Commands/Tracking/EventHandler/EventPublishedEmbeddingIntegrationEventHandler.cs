// using AI.Application.Abstractions.Qdrant;
// using AI.Application.Abstractions.Qdrant.Model;
// using AI.Application.Helpers;
// using Events.IntegrationEvents;
// using Microsoft.Extensions.Logging;
// using Shared.Application.Abstractions.Embbeding;
// using Shared.Application.Abstractions.EventBus;

// namespace Embedding.Application.EventHandlers
// {
//     /// <summary>
//     /// Consumes EventPublishedEmbeddingIntegrationEvent from the Events module.
//     ///
//     /// FLOW:
//     ///   1. Build embedding text from title + categories + hashtags + description
//     ///   2. Call IEmbeddingService to get float[] vector
//     ///   3. Upsert vector + payload into Qdrant via IEventVectorRepository
//     ///
//     /// FIRE AND FORGET SAFETY:
//     ///   If this handler fails, MassTransit will retry based on your retry policy.
//     ///   The Events module already committed to DB — this is best-effort async indexing.
//     /// </summary>
//     internal sealed class EventPublishedEmbeddingIntegrationEventHandler(
//         IEmbeddingService embeddingService,
//         IEventVectorRepository eventVectorRepo,
//         ILogger<EventPublishedEmbeddingIntegrationEventHandler> logger)
//         : IntegrationEventHandler<EventPublishedEmbeddingIntegrationEvent>
//     {
//         public override async Task Handle(
//             EventPublishedEmbeddingIntegrationEvent integrationEvent,
//             CancellationToken cancellationToken = default)
//         {
//             logger.LogInformation(
//                 "Embedding refresh for published event {EventId} — title: {Title}",
//                 integrationEvent.EventId, integrationEvent.Title);

//             // ── 1. Build embedding text ───────────────────────────────
//             var text = EmbeddingTextBuilder.ForEvent(
//                 title:       integrationEvent.Title,
//                 categories:  integrationEvent.Categories.ToList(),
//                 hashtags:    integrationEvent.Hashtags.ToList(),
//                 description: integrationEvent.Description
//             );

//             if (string.IsNullOrWhiteSpace(text))
//             {
//                 logger.LogWarning(
//                     "Skipping embedding refresh for event {EventId} — no text to embed",
//                     integrationEvent.EventId);
//                 return;
//             }

//             // ── 2. Generate embedding ─────────────────────────────────
//             var embedding = await embeddingService.EmbedAsync(text, cancellationToken);

//             // ── 3. Upsert into Qdrant ─────────────────────────────────
//             var payload = new EventVectorPayload(
//                 EventId:      integrationEvent.EventId,
//                 Title:        integrationEvent.Title,
//                 Category:     integrationEvent.Categories.FirstOrDefault(),
//                 Hashtags:     integrationEvent.Hashtags.ToList(),
//                 EventStartAt: integrationEvent.UpdatedAt, // using UpdatedAt for published
//                 MinPrice:     null,
//                 BannerUrl:    null
//             );

//             await eventVectorRepo.UpsertEventAsync(payload, embedding, cancellationToken);

//             logger.LogInformation(
//                 "Published event {EventId} re-indexed in Qdrant — {Dim}-dim vector, categories: [{Categories}]",
//                 integrationEvent.EventId,
//                 embedding.Length,
//                 string.Join(", ", integrationEvent.Categories));
//         }
//     }
// }
