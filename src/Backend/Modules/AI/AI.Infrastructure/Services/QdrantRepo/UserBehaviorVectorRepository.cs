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
/// Stores embeddings of user behavior logs (views, searches, bookmarks, purchases).
///
/// PURPOSE:
///   - BehaviorLogCreatedEvent handler embeds the log and calls UpsertBehaviorAsync.
///   - Recommendation pipeline calls GetRecentVectorsAsync to fetch raw vectors,
///     computes WeightedCentroid weighted by UserInterestScore,
///     then searches EventVectorRepository with the resulting interest vector.
///
/// COLLECTION KEY: "UserBehavior" in Qdrant:Collections config.
/// </summary>
public sealed class UserBehaviorVectorRepository : QdrantRepositoryBase, IUserBehaviorVectorRepository
{
    protected override string CollectionName { get; }
    protected override int VectorSize { get; }

    public UserBehaviorVectorRepository(
        QdrantClient client,
        QdrantConfig config,
        ILogger<UserBehaviorVectorRepository> logger)
        : base(client, logger)
    {
        var col = config.Get("UserBehavior");
        CollectionName = col.Name;
        VectorSize = col.VectorSize;
    }

    // ── Collection Setup ──────────────────────────────────────────

    public override async Task EnsureCollectionAsync(CancellationToken ct = default)
    {
        await base.EnsureCollectionAsync(ct);

        await CreatePayloadIndexAsync("user_id",     PayloadSchemaType.Keyword, ct);
        await CreatePayloadIndexAsync("action_type", PayloadSchemaType.Keyword, ct);
        await CreatePayloadIndexAsync("occurred_at", PayloadSchemaType.Datetime, ct);
        await CreatePayloadIndexAsync("categories",  PayloadSchemaType.Keyword, ct);
        await CreatePayloadIndexAsync("hashtags",    PayloadSchemaType.Keyword, ct);
    }

    // ── Write ─────────────────────────────────────────────────────

    public async Task UpsertBehaviorAsync(
        UserBehaviorVectorPayload log,
        float[] embedding,
        CancellationToken ct = default)
    {
        await UpsertRawAsync(log.LogId, embedding, BuildPayload(log), ct);
        Logger.LogDebug(
            "Upserted behavior vector {LogId} for user {UserId}",
            log.LogId, log.UserId);
    }

    // ── Read ──────────────────────────────────────────────────────

    /// <summary>
    /// Fetch raw embedding vectors for a user's recent behavior logs.
    ///
    /// HOW IT WORKS:
    ///   1. Filter Qdrant by user_id (+ optional since date) using ScrollAsync —
    ///      no query vector is involved, so cosine similarity is not computed.
    ///   2. withVectors: true returns the actual float[] embeddings.
    ///   3. Map to UserBehaviorVector {LogId, Categories, Vector}.
    ///
    /// WHY ScrollAsync instead of SearchAsync:
    ///   SearchAsync with a zero query vector produces undefined cosine similarity
    ///   scores (0-vector has no direction), making result ordering meaningless.
    ///   ScrollAsync retrieves points by filter only — correct and deterministic.
    ///
    /// USED BY: Recommendation pipeline to build WeightedCentroid interest vector.
    /// </summary>
    public async Task<IReadOnlyList<UserBehaviorVector>> GetRecentVectorsAsync(
        Guid userId,
        int limit = 50,
        DateTime? since = null,
        CancellationToken ct = default)
    {
        if (limit <= 0)
            return Array.Empty<UserBehaviorVector>();

        var filter = QdrantFilterBuilder.Must()
            .Keyword("user_id", userId)
            .DateTimeAfter("occurred_at", since)
            .Build();

        var scrollResult = await Client.ScrollAsync(
            collectionName: CollectionName,
            filter:         filter,
            limit:          (uint)limit,
            payloadSelector: true,
            vectorsSelector: true,
            cancellationToken: ct
        );

        return scrollResult.Result
            .Select(p =>
            {
                var reader     = new QdrantPayloadReader(p.Payload);
                var logId      = reader.GetGuid("log_id");
                var categories = reader.GetStringList("categories");
                var vector     = p.Vectors?.Vector?.GetDenseVector()?.Data.ToArray()
                                 ?? Array.Empty<float>();

                return new UserBehaviorVector(logId, categories, vector);
            })
            .Where(v => v.Vector.Length > 0)
            .ToList();
    }

    /// <summary>
    /// Find behavior logs most similar to a query vector, scoped to one user.
    /// Uses SearchAsync because a real query vector is provided here.
    /// </summary>
    public async Task<IReadOnlyList<BehaviorSearchResult>> SearchUserBehaviorAsync(
        Guid userId,
        float[] queryVector,
        int limit = 50,
        float scoreThreshold = 0.3f,
        string? filterAction = null,
        DateTime? afterDate = null,
        CancellationToken ct = default)
    {
        var filter = QdrantFilterBuilder.Must()
            .Keyword("user_id", userId)
            .Keyword("action_type", filterAction)
            .DateTimeAfter("occurred_at", afterDate)
            .Build();

        var hits = await SearchRawAsync(queryVector, filter, limit, ct);

        return hits
            .Where(h => h.Score >= scoreThreshold)
            .Select(MapToResult)
            .ToList();
    }

    /// <summary>
    /// Find behavior logs across ALL users — for collaborative filtering.
    /// Uses SearchAsync because a real query vector is provided here.
    /// </summary>
    public async Task<IReadOnlyList<BehaviorSearchResult>> SearchGlobalBehaviorAsync(
        float[] queryVector,
        int limit = 100,
        float scoreThreshold = 0.4f,
        string? filterAction = null,
        CancellationToken ct = default)
    {
        var filter = QdrantFilterBuilder.Must()
            .Keyword("action_type", filterAction)
            .Build();

        var hits = await SearchRawAsync(queryVector, filter, limit, ct);

        return hits
            .Where(h => h.Score >= scoreThreshold)
            .Select(MapToResult)
            .ToList();
    }

    /// <summary>
    /// Returns category → interaction count for a user's recent behavior.
    /// Used to seed UserInterestScore without a full ML pipeline.
    ///
    /// Uses ScrollAsync to retrieve all matching points by filter —
    /// no similarity scoring needed here, just counting category occurrences.
    /// </summary>
    public async Task<IReadOnlyDictionary<string, int>> GetCategoryFrequencyAsync(
        Guid userId,
        DateTime? since = null,
        string? filterAction = null,
        int sampleSize = 200,
        CancellationToken ct = default)
    {
        if (sampleSize <= 0)
            return new Dictionary<string, int>();

        var filter = QdrantFilterBuilder.Must()
            .Keyword("user_id", userId)
            .Keyword("action_type", filterAction)
            .DateTimeAfter("occurred_at", since)
            .Build();

        // ScrollAsync: we only need payload (categories), not vectors
        var scrollResult = await Client.ScrollAsync(
            collectionName:  CollectionName,
            filter:          filter,
            limit:           (uint)sampleSize,
            payloadSelector: true,
            vectorsSelector: false,   // vectors not needed — saves bandwidth
            cancellationToken: ct
        );

        return scrollResult.Result
            .SelectMany(p => new QdrantPayloadReader(p.Payload).GetStringList("categories"))
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .GroupBy(c => c)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // ── Private Helpers ───────────────────────────────────────────

    private static IDictionary<string, Value> BuildPayload(UserBehaviorVectorPayload log) =>
        new Dictionary<string, Value>
        {
            ["log_id"]      = ToQdrantValue(log.LogId),
            ["user_id"]     = ToQdrantValue(log.UserId),
            ["action_type"] = ToQdrantValue(log.ActionType),
            ["target_id"]   = ToQdrantValue(log.TargetId),
            ["target_type"] = ToQdrantValue(log.TargetType),
            ["categories"]  = ToQdrantValue(log.Categories
                                  .Select(c => c.ToLowerInvariant()).ToList()),
            ["hashtags"]    = ToQdrantValue(log.Hashtags
                                  .Select(h => h.ToLowerInvariant()).ToList()),
            ["session_id"]  = ToQdrantValue(log.SessionId ?? ""),
            ["device_type"] = ToQdrantValue(log.DeviceType ?? ""),
            ["occurred_at"] = ToQdrantValue(log.OccurredAt),
        };

    private static BehaviorSearchResult MapToResult(ScoredPoint hit)
    {
        var r = new QdrantPayloadReader(hit.Payload);
        return new BehaviorSearchResult(
            LogId:      r.GetGuid("log_id"),
            UserId:     r.GetGuid("user_id"),
            Score:      hit.Score,
            ActionType: r.GetString("action_type"),
            TargetId:   r.GetString("target_id"),
            Categories: r.GetStringList("categories"),
            Hashtags:   r.GetStringList("hashtags"),
            OccurredAt: r.GetDateTime("occurred_at")
        );
    }
}