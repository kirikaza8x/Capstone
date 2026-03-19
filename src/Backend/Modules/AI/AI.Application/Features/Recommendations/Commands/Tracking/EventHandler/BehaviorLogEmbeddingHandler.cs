using AI.Application.Abstractions.Qdrant;
using AI.Application.Abstractions.Qdrant.Model;
using AI.Application.Helpers;
using AI.Domain.Events;
using AI.Domain.Helpers;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Embbeding;
using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.Tracking.EventHandlers;

/// <summary>
/// Handles BehaviorLogCreatedEvent — embeds the log text and stores the vector in Qdrant.
///
/// RUNS ALONGSIDE BehaviorLogCreatedEventHandler (interest score update).
/// MediatR dispatches to ALL handlers for the same event — no conflict.
///
/// FLOW:
///   1. Extract categories + hashtags from metadata via MetadataHelper
///   2. Build embedding text via EmbeddingTextBuilder.ForBehaviorLog()
///   3. Call IEmbeddingService → float[384] vector
///   4. Upsert to IUserBehaviorVectorRepository
///
/// PURPOSE: Enables semantic behavior search used by the recommendation pipeline
/// to find what a user engaged with and build their interest vector.
///
/// SAFETY: Exceptions are caught and logged — embedding is a side-effect and
/// must never break the main tracking flow.
/// </summary>
public sealed class BehaviorLogEmbeddingHandler(
    IUserBehaviorVectorRepository behaviorVectorRepo,
    IEmbeddingService embeddingService,
    ILogger<BehaviorLogEmbeddingHandler> logger)
    : IDomainEventHandler<BehaviorLogCreatedEvent>
{
    public async Task Handle(BehaviorLogCreatedEvent @event, CancellationToken ct)
    {
        try
        {
            // ── 1. Extract categories + hashtags from metadata ────
            var meta = new MetadataHelper(
                @event.Metadata ?? new Dictionary<string, string>());

            var categories = meta.GetList(new[] { "categories", "category" });

            var hashtags = meta.GetList(new[] { "hashtags", "hashtag" })
                               .Select(h => h.TrimStart('#'))
                               .Where(h => !string.IsNullOrWhiteSpace(h))
                               .Distinct()
                               .ToList();

            // ── 2. Build embedding text ───────────────────────────
            var text = EmbeddingTextBuilder.ForBehaviorLog(
                actionType: @event.ActionType,
                targetType: @event.TargetType,
                categories: categories,
                hashtags: hashtags
            );

            if (string.IsNullOrWhiteSpace(text))
            {
                logger.LogDebug(
                    "Skipping embedding for log {LogId} — no text to embed",
                    @event.LogId);
                return;
            }

            // ── 3. Generate embedding ─────────────────────────────
            var embedding = await embeddingService.EmbedAsync(text, ct);

            // ── 4. Upsert to Qdrant ───────────────────────────────
            var payload = new UserBehaviorVectorPayload(
                LogId: @event.LogId,
                UserId: @event.UserId,
                ActionType: @event.ActionType,
                TargetId: @event.TargetId,
                TargetType: @event.TargetType,
                Categories: categories,
                Hashtags: hashtags,
                SessionId: null,   // not in domain event — add to BehaviorLogCreatedEvent if needed
                DeviceType: null,   // not in domain event — add to BehaviorLogCreatedEvent if needed
                OccurredAt: @event.OccurredAt
            );

            await behaviorVectorRepo.UpsertBehaviorAsync(payload, embedding, ct);

            logger.LogInformation(
                "Behavior log {LogId} embedded — user {UserId}, action: {Action}, " +
                "categories: [{Categories}], hashtags: [{Hashtags}]",
                @event.LogId, @event.UserId, @event.ActionType,
                string.Join(", ", categories),
                string.Join(", ", hashtags));
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to embed behavior log {LogId} for user {UserId} — skipping",
                @event.LogId, @event.UserId);
        }
    }
}
