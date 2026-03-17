using Qdrant.Client;
using Qdrant.Client.Grpc;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Configs.Qdrant;
using Shared.Infrastructure.Qdrant;
using Shared.Infrastructure.Qdrant.Helpers;
using AI.Application.Abstractions.Qdrant;
using AI.Application.Abstractions.Qdrant.Model;

namespace AI.Infrastructure.Qdrant;



// ── Repository ────────────────────────────────────────────────────────────────

/// <summary>
/// Stores event embeddings and supports semantic similarity search.
///
/// PURPOSE: When an event is created/updated, its embedding is upserted here.
/// Recommendation uses SearchSimilarAsync with a user interest vector or
/// a query embedding to return ranked candidate events.
///
/// COLLECTION KEY: "Events" in Qdrant:Collections config.
///
/// PAYLOAD INDEXES:
///   - category  → keyword  → fast category filtering
///   - start_at  → datetime → filter future-only events
///   - hashtags  → keyword  → tag-based filtering
/// </summary>
public sealed class EventVectorRepository : QdrantRepositoryBase, IEventVectorRepository
{
    protected override string CollectionName { get; }
    protected override int    VectorSize     { get; }

    public EventVectorRepository(
        QdrantClient client,
        QdrantConfig config,
        ILogger<EventVectorRepository> logger)
        : base(client, logger)
    {
        var col = config.Get("Events");
        CollectionName = col.Name;
        VectorSize     = col.VectorSize;
    }

    // ── Collection Setup ──────────────────────────────────────────

    /// <summary>
    /// Creates the collection then adds payload field indexes.
    /// Overrides base to append index creation after collection is ready.
    /// </summary>
    public override async Task EnsureCollectionAsync(CancellationToken ct = default)
    {
        await base.EnsureCollectionAsync(ct);

        // Idempotent — Qdrant ignores if index already exists
        await CreatePayloadIndexAsync("category", PayloadSchemaType.Keyword,  ct);
        await CreatePayloadIndexAsync("start_at", PayloadSchemaType.Datetime, ct);
        await CreatePayloadIndexAsync("hashtags", PayloadSchemaType.Keyword,  ct);
    }

    // ── Write ─────────────────────────────────────────────────────

    /// <summary>
    /// Upsert a single event embedding.
    /// Called from EventCreated / EventUpdated domain event handler.
    /// </summary>
    public async Task UpsertEventAsync(
        EventVectorPayload evt,
        float[]            embedding,
        CancellationToken  ct = default)
    {
        await UpsertRawAsync(evt.EventId, embedding, BuildPayload(evt), ct);
        Logger.LogDebug("Upserted event vector {EventId}", evt.EventId);
    }

    /// <summary>
    /// Bulk upsert — use for initial seeding or re-indexing jobs.
    /// </summary>
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
    /// Semantic similarity search.
    /// Pass a user interest vector for personalised recommendation,
    /// or a query text embedding for semantic search.
    /// </summary>
    public async Task<IReadOnlyList<EventSearchResult>> SearchSimilarAsync(
        float[]                queryEmbedding,
        int                    limit          = 20,
        float                  scoreThreshold = 0.3f,
        string?                filterCategory = null,
        IReadOnlyList<string>? filterHashtags = null,
        DateTime?              afterDate      = null,
        CancellationToken      ct             = default)
    {
        var filter = QdrantFilterBuilder.Must()
            .Keyword("category", filterCategory)
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
            ["event_id"]   = ToQdrantValue(evt.EventId),
            ["title"]      = ToQdrantValue(evt.Title),
            ["category"]   = ToQdrantValue(evt.Category?.ToLowerInvariant() ?? ""),
            ["hashtags"]   = ToQdrantValue(evt.Hashtags.Select(h => h.ToLowerInvariant()).ToList()),
            ["start_at"]   = ToQdrantValue(evt.EventStartAt),
            ["min_price"]  = ToQdrantValue(evt.MinPrice?.ToString() ?? ""),
            ["banner_url"] = ToQdrantValue(evt.BannerUrl ?? ""),
        };

    private static EventSearchResult MapToResult(ScoredPoint hit)
    {
        var r = new QdrantPayloadReader(hit.Payload);
        return new EventSearchResult(
            EventId:      r.GetGuid("event_id"),
            Score:        hit.Score,
            Title:        r.GetString("title"),
            Category:     r.GetStringOrNull("category"),
            Hashtags:     r.GetStringList("hashtags"),
            EventStartAt: r.GetDateTime("start_at"),
            MinPrice:     r.GetDecimal("min_price"),
            BannerUrl:    r.GetStringOrNull("banner_url")
        );
    }
}