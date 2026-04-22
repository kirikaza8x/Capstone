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

        // Create payload indexes with error handling.
        // Note: "InvalidArgument" errors often occur due to Qdrant.Client library 
        // version mismatches with the server (e.g., Datetime type support).
        // We swallow exceptions here to allow the app to start even if indexing fails.
        await CreatePayloadIndexSafeAsync("category", PayloadSchemaType.Keyword, ct);
        await CreatePayloadIndexSafeAsync("hashtags",  PayloadSchemaType.Keyword, ct);
        await CreatePayloadIndexSafeAsync("start_at",  PayloadSchemaType.Datetime, ct);
    }

    private async Task CreatePayloadIndexSafeAsync(string field, PayloadSchemaType type, CancellationToken ct)
    {
        try
        {
            await CreatePayloadIndexAsync(field, type, ct);
            Logger.LogDebug("Created payload index for field '{Field}'", field);
        }
        catch (Exception ex)
        {
            // Log warning but do not crash. The app can still function without this specific index.
            Logger.LogWarning(ex, "Failed to create payload index for field '{Field}'. App will continue without this index.", field);
        }
    }

    // ── Write ─────────────────────────────────────────────────────

    public async Task UpsertEventAsync(
        EventVectorPayload evt,
        float[] embedding,
        CancellationToken ct = default)
    {
        await UpsertRawAsync(evt.EventId, embedding, BuildPayload(evt), ct);
        Logger.LogDebug("Upserted event vector {EventId}", evt.EventId);
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

    /// <summary>
    /// Semantic similarity search — call this only when you have a real query vector
    /// (e.g. WeightedCentroid built from user behavior embeddings).
    ///
    /// Do NOT call with a zero vector just to apply category filters —
    /// use ScrollByFilterAsync instead for those cases.
    /// </summary>
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
            .KeywordAny("hashtags",  filterHashtags)
            .DateTimeAfter("start_at", afterDate)
            .Build();

        var hits = await SearchRawAsync(queryEmbedding, filter, limit, ct);

        return hits
            .Where(h => h.Score >= scoreThreshold)
            .Select(MapToResult)
            .ToList();
    }

    /// <summary>
    /// Filter-only retrieval — no query vector, no similarity scoring.
    ///
    /// Use this for:
    ///   - Category fallback (user has interest scores but no behavior vectors yet)
    ///   - Cold-start (new user — retrieve popular category events by filter only)
    ///
    /// WHY: Calling SearchAsync with a zero vector produces undefined cosine similarity
    /// scores. ScrollAsync retrieves by filter alone — correct and deterministic.
    /// Vectors are not fetched here since the caller does not need them.
    /// </summary>
    public async Task<IReadOnlyList<EventSearchResult>> ScrollByFilterAsync(
        int limit,
        IReadOnlyList<string>? filterCategories = null,
        IReadOnlyList<string>? filterHashtags = null,
        DateTime? afterDate = null,
        CancellationToken ct = default)
    {
        if (limit <= 0)
            return Array.Empty<EventSearchResult>();

        var filter = QdrantFilterBuilder.Must()
            .KeywordAny("category", filterCategories)
            .KeywordAny("hashtags",  filterHashtags)
            .DateTimeAfter("start_at", afterDate)
            .Build();

        var scrollResult = await Client.ScrollAsync(
            collectionName:  CollectionName,
            filter:          filter,
            limit:           (uint)limit,
            payloadSelector: true,
            vectorsSelector: false,   // no vectors needed — saves bandwidth
            cancellationToken: ct
        );

        return scrollResult.Result
            .Select(MapScrollToResult)
            .ToList();
    }

    public async Task<IReadOnlyList<Guid>> GetAllEventIdsAsync(CancellationToken ct = default)
    {
        var allPoints = new List<Guid>();
        uint offset = 0;
        const uint batchSize = 1000;

        while (true)
        {
            var scrollResult = await Client.ScrollAsync(
                collectionName: CollectionName,
                filter: null,  // No filter to get all points
                limit: batchSize,
                offset: offset > 0 ? new PointId { Num = offset } : null,
                payloadSelector: true,
                vectorsSelector: false,
                cancellationToken: ct
            );

            if (scrollResult.Result.Count == 0)
                break;

            foreach (var point in scrollResult.Result)
            {
                var reader = new QdrantPayloadReader(point.Payload);
                allPoints.Add(reader.GetGuid("event_id"));
            }

            if (scrollResult.Result.Count < batchSize)
                break;

            offset += batchSize;
        }

        return allPoints;
    }

    private static IDictionary<string, Value> BuildPayload(EventVectorPayload evt) =>
        new Dictionary<string, Value>
        {
            ["event_id"]   = ToQdrantValue(evt.EventId),
            ["title"]      = ToQdrantValue(evt.Title),
            ["category"]   = ToQdrantValue(evt.Categories
                                 .Select(c => c.ToLowerInvariant())
                                 .ToList()),
            ["hashtags"]   = ToQdrantValue(evt.Hashtags
                                 .Select(h => h.ToLowerInvariant())
                                 .ToList()),
            ["start_at"]   = ToQdrantValue(evt.EventStartAt),
            ["min_price"]  = ToQdrantValue(evt.MinPrice?.ToString() ?? ""),
            ["banner_url"] = ToQdrantValue(evt.BannerUrl ?? ""),
        };

    // Maps a ScoredPoint (SearchAsync result) — has a Score field
    private static EventSearchResult MapToResult(ScoredPoint hit)
    {
        var r = new QdrantPayloadReader(hit.Payload);
        return new EventSearchResult(
            EventId:      r.GetGuid("event_id"),
            Score:        hit.Score,
            Title:        r.GetString("title"),
            Categories:   r.GetStringList("category"),
            Hashtags:     r.GetStringList("hashtags"),
            EventStartAt: r.GetDateTime("start_at"),
            MinPrice:     r.GetDecimal("min_price"),
            BannerUrl:    r.GetStringOrNull("banner_url")
        );
    }

    // Maps a RetrievedPoint (ScrollAsync result) — no Score, defaults to 0
    private static EventSearchResult MapScrollToResult(RetrievedPoint p)
    {
        var r = new QdrantPayloadReader(p.Payload);
        return new EventSearchResult(
            EventId:      r.GetGuid("event_id"),
            Score:        0f,   // no similarity score — filter-only retrieval
            Title:        r.GetString("title"),
            Categories:   r.GetStringList("category"),
            Hashtags:     r.GetStringList("hashtags"),
            EventStartAt: r.GetDateTime("start_at"),
            MinPrice:     r.GetDecimal("min_price"),
            BannerUrl:    r.GetStringOrNull("banner_url")
        );
    }
}