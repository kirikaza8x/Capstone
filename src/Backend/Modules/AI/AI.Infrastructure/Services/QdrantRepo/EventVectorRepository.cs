using AI.Application.Abstractions.Qdrant;
using AI.Application.Abstractions.Qdrant.Model;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Shared.Infrastructure.Configs.Qdrant;
using Shared.Infrastructure.Qdrant;
using Shared.Infrastructure.Qdrant.Helpers;

namespace AI.Infrastructure.Qdrant;

/// <summary>
/// Stores event embeddings and supports semantic similarity search.
/// DTOs live in AI.Application.Abstractions.Qdrant.Model — not here.
///
/// COLLECTION KEY: "Events" in Qdrant:Collections config.
/// PAYLOAD INDEXES: category (keyword), hashtags (keyword), start_at (datetime)
/// </summary>
public sealed class EventVectorRepository : QdrantRepositoryBase, IEventVectorRepository
{
    protected override string CollectionName { get; }
    protected override int VectorSize { get; }

    public EventVectorRepository(
        QdrantClient client,
        QdrantConfig config,
        ILogger<EventVectorRepository> logger)
        : base(client, logger)
    {
        var col = config.Get("Events");
        CollectionName = col.Name;
        VectorSize = col.VectorSize;
    }

    // ── Collection Setup ──────────────────────────────────────────

    public override async Task EnsureCollectionAsync(CancellationToken ct = default)
    {
        await base.EnsureCollectionAsync(ct);
        await CreatePayloadIndexAsync("category", PayloadSchemaType.Keyword, ct);
        await CreatePayloadIndexAsync("hashtags", PayloadSchemaType.Keyword, ct);
        await CreatePayloadIndexAsync("start_at", PayloadSchemaType.Datetime, ct);
    }

    // ── Write ─────────────────────────────────────────────────────

    public async Task UpsertEventAsync(
        EventVectorPayload evt,
        float[] embedding,
        CancellationToken ct = default)
    {
        await UpsertRawAsync(evt.EventId, embedding, BuildPayload(evt), ct);
        Logger.LogDebug("Upserted event vector {EventId}", evt.EventId);
        // Note: UpsertRawAsync handles both insert and update — Qdrant will create or overwrite the point with the given ID.
    }

    public async Task UpsertBatchAsync(
        IEnumerable<(EventVectorPayload Event, float[] Embedding)> items,
        CancellationToken ct = default)
    {
        var batch = items.Select(x => (x.Event.EventId, x.Embedding, BuildPayload(x.Event)));
        await UpsertBatchRawAsync(batch, ct);
        Logger.LogInformation("Bulk upserted event vectors");
    }

    // ── Read ──────────────────────────────────────────────────────

    public async Task<IReadOnlyList<EventSearchResult>> SearchSimilarAsync(
        float[] queryEmbedding,
        int limit = 20,
        float scoreThreshold = 0.3f,
        IReadOnlyList<string>? filterCategories = null,
        IReadOnlyList<string>? filterHashtags = null,
        DateTime? afterDate = null,
        CancellationToken ct = default)
    {
        var filter = QdrantFilterBuilder.Must()
            .KeywordAny("category", filterCategories)
            .KeywordAny("hashtags", filterHashtags)
            .DateTimeAfter("start_at", afterDate)
            .Build();

        var hits = await SearchRawAsync(queryEmbedding, filter, limit, ct);

        return hits
            .Where(h => h.Score >= scoreThreshold)
            .Select(MapToResult)
            .ToList();
    }

    // ── Private Helpers ───────────────────────────────────────────

    private static IDictionary<string, Value> BuildPayload(EventVectorPayload evt) =>
        new Dictionary<string, Value>
        {
            ["event_id"] = ToQdrantValue(evt.EventId),
            ["title"] = ToQdrantValue(evt.Title),
            ["category"] = ToQdrantValue(evt.Categories
                                .Select(c => c.ToLowerInvariant())
                                .ToList()),
            ["hashtags"] = ToQdrantValue(evt.Hashtags
                                .Select(h => h.ToLowerInvariant())
                                .ToList()),
            ["start_at"] = ToQdrantValue(evt.EventStartAt),
            ["min_price"] = ToQdrantValue(evt.MinPrice?.ToString() ?? ""),
            ["banner_url"] = ToQdrantValue(evt.BannerUrl ?? ""),
        };

    private static EventSearchResult MapToResult(ScoredPoint hit)
    {
        var r = new QdrantPayloadReader(hit.Payload);
        return new EventSearchResult(
            EventId: r.GetGuid("event_id"),
            Score: hit.Score,
            Title: r.GetString("title"),
            Categories: r.GetStringList("category"),
            Hashtags: r.GetStringList("hashtags"),
            EventStartAt: r.GetDateTime("start_at"),
            MinPrice: r.GetDecimal("min_price"),
            BannerUrl: r.GetStringOrNull("banner_url")
        );
    }
}
