using AI.Application.Abstractions.Qdrant;
using AI.Application.Features.Recommendations.Queries;
using AI.Application.Features.Recommendations.Services;
using AI.Domain.Helpers;
using AI.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace AI.Application.Features.Recommendations.Handlers;

/// <summary>
/// Semantic recommendation pipeline with re-ranking.
///
/// THREE PATHS (tried in order, first with results wins):
///
/// 1. SEMANTIC — fetch behavior vectors from Qdrant, WeightedCentroid, cosine search + re-rank
/// 2. CATEGORY FALLBACK — user has interest scores but no behavior vectors yet
/// 3. COLD-START — brand new user, popularity-based
/// </summary>
public sealed class GetRecommendationsQueryHandler(
    IUserBehaviorVectorRepository           behaviorVectorRepo,
    IEventVectorRepository                  eventVectorRepo,
    IUserInterestScoreRepository            interestScoreRepo,
    IGlobalCategoryStatRepository           globalStatRepo,
    ILogger<GetRecommendationsQueryHandler> logger)
    : IQueryHandler<GetRecommendationsQuery, List<EventRecommendationResult>>
{
    private const float ScoreThreshold = 0.3f;
    private const int   MaxCategories  = 10;
    private const int   ColdStartTopN  = 5;
    private const int   CandidateCount = 50;
    private const int   VectorDim      = 384;
    private const int   MaxPerCategory = 3;
    private const int   BehaviorLimit  = 50;

    public async Task<Result<List<EventRecommendationResult>>> Handle(
        GetRecommendationsQuery request,
        CancellationToken ct)
    {
        try
        {
            var results = await TrySemanticPathAsync(request, ct);

            if (results.Count == 0)
                results = await TryCategoryFallbackAsync(request, ct);

            if (results.Count == 0)
                results = await TryColdStartFallbackAsync(request, ct);

            logger.LogInformation(
                "Recommendations — UserId={UserId}, Count={Count}, Source={Source}",
                request.UserId, results.Count,
                results.FirstOrDefault()?.Source ?? "none");

            return Result.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Recommendation failed for UserId={UserId}", request.UserId);
            return Result.Failure<List<EventRecommendationResult>>(
                Error.Failure("Recommendations.Failed", "Unable to fetch recommendations."));
        }
    }

    // ── Path 1: Semantic ──────────────────────────────────────────

    private async Task<List<EventRecommendationResult>> TrySemanticPathAsync(
        GetRecommendationsQuery request,
        CancellationToken ct)
    {
        // Fetch raw behavior vectors — actual embeddings of what user engaged with
        var behaviorVectors = await behaviorVectorRepo.GetRecentVectorsAsync(
            userId: request.UserId,
            limit:  BehaviorLimit,
            since:  DateTime.UtcNow.AddDays(-90),
            ct:     ct
        );

        if (behaviorVectors.Count == 0)
            return new List<EventRecommendationResult>();

        // Load interest scores to weight each behavior vector
        var interestScores = await interestScoreRepo.GetTopCategoriesAsync(
            userId:  request.UserId,
            topN:    MaxCategories,
            minScore: 0,
            ct:      ct
        );

        var scoreByCategory = interestScores
            .ToDictionary(s => s.Category, s => s.Score);

        // Weight each vector by average interest score of its categories
        var weightedVectors = behaviorVectors
            .Select(bv =>
            {
                var weight = bv.Categories.Count > 0
                    ? bv.Categories
                        .Select(c => scoreByCategory.TryGetValue(c, out var s) ? s : 1.0)
                        .Average()
                    : 1.0;
                return (Vector: bv.Vector, Weight: weight);
            })
            .Where(x => x.Vector.Length == VectorDim)
            .ToList();

        if (weightedVectors.Count == 0)
            return new List<EventRecommendationResult>();

        float[] interestVector;
        try
        {
            interestVector = VectorMath.WeightedCentroid(
                weightedVectors.Select(v => (v.Vector, v.Weight)));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "WeightedCentroid failed for UserId={UserId}", request.UserId);
            return new List<EventRecommendationResult>();
        }

        var afterDate = request.FutureOnly ? DateTime.UtcNow : (DateTime?)null;

        var hits = await eventVectorRepo.SearchSimilarAsync(
            queryEmbedding: interestVector,
            limit:          CandidateCount,
            scoreThreshold: ScoreThreshold,
            afterDate:      afterDate,
            ct:             ct
        );

        if (hits.Count == 0)
            return new List<EventRecommendationResult>();

        var ranked = RecommendationRanker.Rank(
            hits, scoreByCategory,
            topN:           request.TopN,
            maxPerCategory: MaxPerCategory);

        return ranked
            .Select(r => new EventRecommendationResult(
                r.EventId, r.FinalScore, r.SemanticScore, "semantic"))
            .ToList();
    }

    // ── Path 2: Category Fallback ─────────────────────────────────

    private async Task<List<EventRecommendationResult>> TryCategoryFallbackAsync(
        GetRecommendationsQuery request,
        CancellationToken ct)
    {
        var topScores = await interestScoreRepo.GetTopCategoriesAsync(
            userId:   request.UserId,
            topN:     MaxCategories,
            minScore: 0,
            ct:       ct
        );

        if (topScores.Count == 0)
            return new List<EventRecommendationResult>();

        var categoryNames = topScores.Select(c => c.Category).ToList();
        var afterDate     = request.FutureOnly ? DateTime.UtcNow : (DateTime?)null;

        var hits = await eventVectorRepo.SearchSimilarAsync(
            queryEmbedding:   new float[VectorDim],
            limit:            CandidateCount,
            scoreThreshold:   0f,
            filterCategories: categoryNames,
            afterDate:        afterDate,
            ct:               ct
        );

        if (hits.Count == 0)
            return new List<EventRecommendationResult>();

        var categoryWeights = topScores.ToDictionary(s => s.Category, s => s.Score);
        var ranked = RecommendationRanker.Rank(
            hits, categoryWeights,
            topN:           request.TopN,
            maxPerCategory: MaxPerCategory);

        return ranked
            .Select(r => new EventRecommendationResult(
                r.EventId, r.FinalScore, r.SemanticScore, "category_fallback"))
            .ToList();
    }

    // ── Path 3: Cold-Start ────────────────────────────────────────

    private async Task<List<EventRecommendationResult>> TryColdStartFallbackAsync(
        GetRecommendationsQuery request,
        CancellationToken ct)
    {
        var popular   = await globalStatRepo.GetTopCategoriesAsync(ColdStartTopN);
        var afterDate = request.FutureOnly ? DateTime.UtcNow : (DateTime?)null;

        if (popular.Count == 0)
            return new List<EventRecommendationResult>();

        var hits = await eventVectorRepo.SearchSimilarAsync(
            queryEmbedding:   new float[VectorDim],
            limit:            request.TopN,
            scoreThreshold:   0f,
            filterCategories: popular.Select(c => c.Category).ToList(),
            afterDate:        afterDate,
            ct:               ct
        );

        return hits
            .Select(h => new EventRecommendationResult(
                h.EventId, 0f, 0f, "popular_fallback"))
            .ToList();
    }
}