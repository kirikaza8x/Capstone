using AI.Application.Abstractions;
using AI.Application.Abstractions.Qdrant;
using AI.Application.Abstractions.Qdrant.Model;
using AI.Application.Helpers;
using Events.PublicApi.PublicApi;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Embbeding;

namespace AI.Application.Services;

/// <summary>
/// Re-indexes events from the Events module into Qdrant.
/// Used by both the scheduled job and the manual admin endpoint.
/// </summary>
public sealed class EventReIndexService(
    IEventMemberPublicApi eventApi,
    IEventVectorRepository eventVectorRepo,
    IEmbeddingService embeddingService,
    ILogger<EventReIndexService> logger)
    : IEventReIndexService
{
    private const int PageSize = 100;

    public async Task<int> ReIndexAllAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Starting full event re-index...");

        int page = 1;
        int totalCount = 0;
        int errorCount = 0;

        while (true)
        {
            var events = await eventApi.GetAllForReIndexAsync(page, PageSize, ct);
            if (events.Count == 0) break;

            logger.LogInformation(
                "Re-indexing page {Page} — {Count} events", page, events.Count);

            // Build batch: embed all texts in one call (more efficient)
            var texts = events
                .Select(e => EmbeddingTextBuilder.ForEvent(
                    title: e.Title,
                    categories: e.Categories.ToList() ?? new List<string>(),
                    hashtags: e.Hashtags.ToList() ?? new List<string>(),
                    description: null))
                .ToList();

            IReadOnlyList<float[]> embeddings;
            try
            {
                embeddings = await embeddingService.EmbedBatchAsync(texts, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Batch embedding failed for page {Page} — skipping", page);
                page++;
                errorCount += events.Count;
                continue;
            }

            // Upsert all as a batch
            var batch = events
                .Select((e, i) => (
                    new EventVectorPayload(
                        EventId: e.Id,
                        Title: e.Title,
                        Categories: e.Categories?.ToList() ?? new List<string>(),
                        Hashtags: e.Hashtags?.ToList() ?? new List<string>(),
                        EventStartAt: e.EventStartAt ?? DateTime.UtcNow.AddYears(1),
                        MinPrice: e.MinPrice,
                        BannerUrl: e.BannerUrl
                    ),
                    embeddings[i]
                ))
                .Select(x => (Event: x.Item1, Embedding: x.Item2));

            try
            {
                await eventVectorRepo.UpsertBatchAsync(batch, ct);
                totalCount += events.Count;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Batch upsert failed for page {Page}", page);
                errorCount += events.Count;
            }

            if (events.Count < PageSize) break; // last page
            page++;
        }

        logger.LogInformation(
            "Re-index complete — {Total} indexed, {Errors} errors",
            totalCount, errorCount);

        return totalCount;
    }

    public async Task ReIndexOneAsync(Guid eventId, CancellationToken ct = default)
    {
        // Fetch single event via category filter workaround
        // (public API doesn't have GetByIdAsync — use GetAllForReIndexAsync page 1
        //  and find by ID, or add GetByIdAsync to the public API)
        var page1 = await eventApi.GetAllForReIndexAsync(1, 1000, ct);
        var evt = page1.FirstOrDefault(e => e.Id == eventId);

        if (evt is null)
        {
            logger.LogWarning("ReIndexOne: event {EventId} not found", eventId);
            return;
        }

        var text = EmbeddingTextBuilder.ForEvent(
            title: evt.Title,
            categories: evt.Categories.ToList() ?? new List<string>(),
            hashtags: evt.Hashtags.ToList() ?? new List<string>(),
            description: null);

        var embedding = await embeddingService.EmbedAsync(text, ct);

        await eventVectorRepo.UpsertEventAsync(
            new EventVectorPayload(
                EventId: evt.Id,
                Title: evt.Title,
                Categories: evt.Categories?.ToList() ?? new List<string>(),
                Hashtags: evt.Hashtags?.ToList() ?? new List<string>(),
                EventStartAt: evt.EventStartAt ?? DateTime.UtcNow.AddYears(1),
                MinPrice: evt.MinPrice,
                BannerUrl: evt.BannerUrl
            ),
            embedding, ct);

        logger.LogInformation("Re-indexed event {EventId}", eventId);
    }
}