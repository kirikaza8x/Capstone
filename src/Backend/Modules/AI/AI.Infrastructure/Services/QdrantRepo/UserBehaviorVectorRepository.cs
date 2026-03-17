using Qdrant.Client;
using Qdrant.Client.Grpc;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Configs.Qdrant;
using Shared.Infrastructure.Qdrant;
using Shared.Infrastructure.Qdrant.Helpers;
using AI.Application.Abstractions.Qdrant;
using AI.Application.Abstractions.Qdrant.Model;

namespace AI.Infrastructure.Qdrant;

// ── DTOs ─────────────────────────────────────────────────────────────────────



// ── Repository ────────────────────────────────────────────────────────────────

/// <summary>
/// Stores embeddings of user behavior logs (views, searches, bookmarks, purchases).
///
/// PURPOSE:
///   - BehaviorLogCreated handler embeds the log text and calls UpsertBehaviorAsync.
///   - Recommendation pipeline calls SearchUserBehaviorAsync to find what a user
///     engaged with, averages those vectors → user interest vector →
///     passes to EventVectorRepository.SearchSimilarAsync.
///   - GetCategoryFrequencyAsync seeds UserInterestScore without a full ML job.
///
/// EMBEDDING TEXT (built in application layer):
///   "{ActionType} {TargetType} {Categories.Join(" ")} {Hashtags.Join(" ")}"
///
/// COLLECTION KEY: "UserBehavior" in Qdrant:Collections config.
/// </summary>
public sealed class UserBehaviorVectorRepository : QdrantRepositoryBase, IUserBehaviorVectorRepository
{
    protected override string CollectionName { get; }
    protected override int    VectorSize     { get; }

    public UserBehaviorVectorRepository(
        QdrantClient client,
        QdrantConfig config,
        ILogger<UserBehaviorVectorRepository> logger)
        : base(client, logger)
    {
        var col = config.Get("UserBehavior");
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

        await CreatePayloadIndexAsync("user_id",     PayloadSchemaType.Keyword,  ct);
        await CreatePayloadIndexAsync("action_type", PayloadSchemaType.Keyword,  ct);
        await CreatePayloadIndexAsync("occurred_at", PayloadSchemaType.Datetime, ct);
        await CreatePayloadIndexAsync("categories",  PayloadSchemaType.Keyword,  ct);
        await CreatePayloadIndexAsync("hashtags",    PayloadSchemaType.Keyword,  ct);
    }

    // ── Write ─────────────────────────────────────────────────────

    /// <summary>
    /// Store a single behavior log embedding.
    /// Called from the BehaviorLogCreated domain event handler.
    /// </summary>
    public async Task UpsertBehaviorAsync(
        UserBehaviorVectorPayload log,
        float[]                   embedding,
        CancellationToken         ct = default)
    {
        await UpsertRawAsync(log.LogId, embedding, BuildPayload(log), ct);
        Logger.LogDebug("Upserted behavior vector {LogId} for user {UserId}", log.LogId, log.UserId);
    }

    // ── Read ──────────────────────────────────────────────────────

    /// <summary>
    /// Find behavior logs most similar to a query vector, scoped to one user.
    /// Primary use: build a user interest vector for recommendation by averaging results.
    /// </summary>
    public async Task<IReadOnlyList<BehaviorSearchResult>> SearchUserBehaviorAsync(
        Guid              userId,
        float[]           queryVector,
        int               limit          = 50,
        float             scoreThreshold = 0.3f,
        string?           filterAction   = null,
        DateTime?         afterDate      = null,
        CancellationToken ct             = default)
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
    /// Find behavior logs across ALL users similar to a query vector.
    /// Use case: collaborative filtering.
    /// </summary>
    public async Task<IReadOnlyList<BehaviorSearchResult>> SearchGlobalBehaviorAsync(
        float[]           queryVector,
        int               limit          = 100,
        float             scoreThreshold = 0.4f,
        string?           filterAction   = null,
        CancellationToken ct             = default)
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
    /// Used to seed/update UserInterestScore without a full ML pipeline.
    /// </summary>
    public async Task<IReadOnlyDictionary<string, int>> GetCategoryFrequencyAsync(
        Guid              userId,
        DateTime?         since        = null,
        string?           filterAction = null,
        int               sampleSize   = 200,
        CancellationToken ct           = default)
    {
        var filter = QdrantFilterBuilder.Must()
            .Keyword("user_id", userId)
            .Keyword("action_type", filterAction)
            .DateTimeAfter("occurred_at", since)
            .Build();

        // Zero vector — we want all matching points, not similarity ranking.
        // For production volume, add ScrollRawAsync to base class using Client.ScrollAsync.
        var zeroVector = new float[VectorSize];
        var hits = await SearchRawAsync(zeroVector, filter, sampleSize, ct);

        return hits
            .SelectMany(h => new QdrantPayloadReader(h.Payload).GetStringList("categories"))
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
            ["categories"]  = ToQdrantValue(log.Categories.Select(c => c.ToLowerInvariant()).ToList()),
            ["hashtags"]    = ToQdrantValue(log.Hashtags.Select(h => h.ToLowerInvariant()).ToList()),
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