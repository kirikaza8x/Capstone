using AI.Application.Abstractions.Qdrant;
using AI.Application.Abstractions.Qdrant.Model;
using AI.Application.Helpers;
using Events.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Embbeding;
using Shared.Application.Abstractions.EventBus;

namespace AI.Application.Features.Recommendations.Commands.Tracking.EventHandler;

/// <summary>
/// Consumes EventCreatedEmbeddingIntegrationEvent from the Events module.
///
/// FLOW:
///   1. Build embedding text from title + categories + hashtags + description
///   2. Call IEmbeddingService to get float[] vector (Python service via MassTransit)
///   3. Upsert vector + payload into Qdrant via IEventVectorRepository
///
/// FIRE AND FORGET SAFETY:
///   If this handler fails, MassTransit will retry based on your retry policy.
///   The Events module already committed to DB — this is a best-effort async indexing.
///   A failed upsert means the event won't appear in semantic search until retry succeeds.
/// </summary>
public sealed class EventChangedEmbeddingIntegrationEventHandler(
    IEmbeddingService      embeddingService,
    IEventVectorRepository eventVectorRepo,
    ILogger<EventChangedEmbeddingIntegrationEventHandler> logger)
    : IntegrationEventHandler<EventChangedEmbeddingIntegrationEvent>
{
    public override async Task Handle(
        EventChangedEmbeddingIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Embedding event {EventId} — title: {Title}",
            integrationEvent.EventId, integrationEvent.Title);

        // ── 1. Build embedding text ───────────────────────────────
        var text = EmbeddingTextBuilder.ForEvent(
            title:       integrationEvent.Title,
            categories:  integrationEvent.Categories.ToList(),
            hashtags:    integrationEvent.Hashtags.ToList(),
            description: integrationEvent.Description
        );

        if (string.IsNullOrWhiteSpace(text))
        {
            logger.LogWarning(
                "Skipping embedding for event {EventId} — no text to embed",
                integrationEvent.EventId);
            return;
        }

        // ── 2. Generate embedding ─────────────────────────────────
        var embedding = await embeddingService.EmbedAsync(text, cancellationToken);

        // ── 3. Upsert into Qdrant ─────────────────────────────────
        var payload = new EventVectorPayload(
            EventId:      integrationEvent.EventId,
            Title:        integrationEvent.Title,
            Category:     integrationEvent.Categories.FirstOrDefault(),
            Hashtags:     integrationEvent.Hashtags.ToList(),
            EventStartAt: integrationEvent.CreatedAt ?? DateTime.UtcNow,
            MinPrice:     null,   // not in integration event — enrich later if needed
            BannerUrl:    null    // not in integration event — enrich later if needed
        );

        await eventVectorRepo.UpsertEventAsync(payload, embedding, cancellationToken);

        logger.LogInformation(
            "Event {EventId} indexed in Qdrant — {Dim}-dim vector, categories: [{Categories}]",
            integrationEvent.EventId,
            embedding.Length,
            string.Join(", ", integrationEvent.Categories));
    }
}