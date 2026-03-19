using AI.Application.Abstractions.Qdrant;
using AI.Application.Abstractions.Qdrant.Model;
using AI.Application.Helpers;
using Events.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Embbeding;
using Shared.Application.Abstractions.EventBus;

namespace AI.Application.Features.Recommendations.Commands.Tracking.EventHandler;

/// <summary>
/// Consumes EventChangedEmbeddingIntegrationEvent from the Events module.
///
/// FLOW:
///   1. Build embedding text from title + categories + hashtags + description
///   2. Call IEmbeddingService → float[384] vector via Python HTTP/RabbitMQ
///   3. Upsert vector + payload into Qdrant via IEventVectorRepository
///
/// MUST BE PUBLIC — Scrutor scan and MassTransit consumer registration
/// both require the class to be publicly visible.
/// </summary>
public sealed class EventChangedEmbeddingIntegrationEventHandler(
    IEmbeddingService embeddingService,
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
            title: integrationEvent.Title,
            categories: integrationEvent.Categories.ToList(),
            hashtags: integrationEvent.Hashtags.ToList(),
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
            EventId: integrationEvent.EventId,
            Title: integrationEvent.Title,
            Categories: integrationEvent.Categories.ToList(),
            Hashtags: integrationEvent.Hashtags.ToList(),
            // Use actual event start date if set, otherwise 1 year out for drafts
            // so FutureOnly=true doesn't filter out draft/unscheduled events
            EventStartAt: integrationEvent.EventStartAt
                          ?? DateTime.UtcNow.AddYears(1),
            MinPrice: null,
            BannerUrl: null
        );

        await eventVectorRepo.UpsertEventAsync(payload, embedding, cancellationToken);

        logger.LogInformation(
            "Event {EventId} indexed in Qdrant — {Dim}-dim vector, " +
            "categories: [{Categories}], hashtags: [{Hashtags}]",
            integrationEvent.EventId,
            embedding.Length,
            string.Join(", ", integrationEvent.Categories),
            string.Join(", ", integrationEvent.Hashtags));
    }
}