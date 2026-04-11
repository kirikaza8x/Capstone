using AI.Application.Abstractions.Qdrant;
using AI.Application.Features.Recommendations.Queries;
using AI.Application.Features.Recommendations.Services;
using AI.Domain.Helpers;
using AI.Domain.Repositories;
using Events.PublicApi.PublicApi;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace AI.Application.Features.Recommendations.Handlers;

public sealed class GetRecommendationsQueryHandler(
    IUserBehaviorVectorRepository           behaviorVectorRepo,
    IEventVectorRepository                  eventVectorRepo,
    IUserInterestScoreRepository            interestScoreRepo,
    IGlobalCategoryStatRepository           globalStatRepo,
    IEventMemberPublicApi                   eventApi,
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
            var ranked = await TrySemanticPathAsync(request, ct);

            if (ranked.Count == 0)
                ranked = await TryCategoryFallbackAsync(request, ct);

            if (ranked.Count == 0)
                ranked = await TryColdStartFallbackAsync(request, ct);

            if (ranked.Count == 0)
            {
                logger.LogInformation("No recommendations found for UserId={UserId}", request.UserId);
                return Result.Success(new List<EventRecommendationResult>());
            }

            var eventIds  = ranked.Select(r => r.EventId).ToList();
            var details   = await eventApi.GetMiniByIdsAsync(eventIds, ct);
            var detailMap = details.ToDictionary(d => d.Id);

            var results = ranked
                .Where(r => detailMap.ContainsKey(r.EventId))
                .Select(r =>
                {
                    var d = detailMap[r.EventId];
                    return new EventRecommendationResult(
                        EventId:       r.EventId,
                        FinalScore:    r.FinalScore,
                        SemanticScore: r.SemanticScore,
                        Source:        r.Source,
                        Title:         d.Title,
                        BannerUrl:     d.BannerUrl,
                        Location:      d.Location,
                        EventStartAt:  d.EventStartAt,
                        EventEndAt:    d.EventEndAt,
                        MinPrice:      d.MinPrice,
                        MaxPrice:      d.MaxPrice
                    );
                })
                .ToList();

            logger.LogInformation(
                "Recommendations — UserId={UserId}, Count={Count}, Source={Source}",
                request.UserId, results.Count,
                results.FirstOrDefault()?.Source ?? "none");

            return Result.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Recommendation failed for UserId={UserId}", request.UserId);
            return Result.Failure<List<EventRecommendationResult>>(
                Error.Failure("Recommendations.Failed", "Unable to fetch recommendations."));
        }
    }

    // ── Path 1: Semantic ──────────────────────────────────────────
    // Requires real behavior vectors to build a WeightedCentroid query vector.
    // Falls through to Path 2 if the user has no behavior history in Qdrant.

    private async Task<List<RankedEvent>> TrySemanticPathAsync(
        GetRecommendationsQuery request,
        CancellationToken ct)
    {
        // STEP 1 — fetch behavior vectors
        var behaviorVectors = await behaviorVectorRepo.GetRecentVectorsAsync(
            userId: request.UserId,
            limit:  BehaviorLimit,
            since:  null,
            ct:     ct);

        logger.LogInformation(
            "[Semantic] Step1 BehaviorVectors={Count} for UserId={UserId}",
            behaviorVectors.Count, request.UserId);

        if (behaviorVectors.Count == 0)
            return new List<RankedEvent>();

        // STEP 2 — load interest scores
        var interestScores = await interestScoreRepo.GetTopCategoriesAsync(
            request.UserId, topN: MaxCategories, minScore: 0, ct: ct);

        logger.LogInformation(
            "[Semantic] Step2 InterestScores={Count} [{Scores}]",
            interestScores.Count,
            string.Join(", ", interestScores.Select(s => $"{s.Category}:{s.Score:F1}")));

        var scoreByCategory = interestScores
            .ToDictionary(s => s.Category, s => s.Score);

        // STEP 3 — build weighted vectors
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

        logger.LogInformation(
            "[Semantic] Step3 WeightedVectors={Count} (filtered from {Total} by dim={Dim})",
            weightedVectors.Count, behaviorVectors.Count, VectorDim);

        if (weightedVectors.Count == 0)
            return new List<RankedEvent>();

        // STEP 4 — WeightedCentroid
        float[] interestVector;
        try
        {
            interestVector = VectorMath.WeightedCentroid(
                weightedVectors.Select(v => (v.Vector, v.Weight)));

            logger.LogInformation(
                "[Semantic] Step4 InterestVector built — dim={Dim}, norm={Norm:F4}",
                interestVector.Length,
                Math.Sqrt(interestVector.Sum(x => (double)x * x)));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[Semantic] Step4 WeightedCentroid FAILED for UserId={UserId}", request.UserId);
            return new List<RankedEvent>();
        }

        // STEP 5 — Qdrant semantic search (real query vector — SearchSimilarAsync is correct here)
        var afterDate = request.FutureOnly ? DateTime.UtcNow : (DateTime?)null;

        var hits = await eventVectorRepo.SearchSimilarAsync(
            queryEmbedding: interestVector,
            limit:          CandidateCount,
            scoreThreshold: ScoreThreshold,
            afterDate:      afterDate,
            ct:             ct);

        logger.LogInformation(
            "[Semantic] Step5 QdrantHits={Count} (threshold={Threshold}, futureOnly={FutureOnly})",
            hits.Count, ScoreThreshold, request.FutureOnly);

        if (hits.Count == 0)
            return new List<RankedEvent>();

        // STEP 6 — re-rank
        var ranked = RecommendationRanker.Rank(
            hits, scoreByCategory,
            topN: request.TopN, maxPerCategory: MaxPerCategory);

        logger.LogInformation(
            "[Semantic] Step6 RankedResults={Count}", ranked.Count);

        return ranked
            .Select(r => new RankedEvent(r.EventId, r.FinalScore, r.SemanticScore, "semantic"))
            .ToList();
    }

    // ── Path 2: Category Fallback ─────────────────────────────────
    // User has interest scores (from postgres) but no behavior vectors in Qdrant yet.
    // Uses ScrollByFilterAsync — filter by category only, no query vector involved.

    private async Task<List<RankedEvent>> TryCategoryFallbackAsync(
        GetRecommendationsQuery request,
        CancellationToken ct)
    {
        var topScores = await interestScoreRepo.GetTopCategoriesAsync(
            request.UserId, topN: MaxCategories, minScore: 0, ct: ct);

        logger.LogInformation(
            "[CategoryFallback] InterestScores={Count}", topScores.Count);

        if (topScores.Count == 0)
            return new List<RankedEvent>();

        var categoryNames = topScores.Select(c => c.Category).ToList();
        var afterDate     = request.FutureOnly ? DateTime.UtcNow : (DateTime?)null;

        // ScrollByFilterAsync: no query vector — retrieves by category filter only
        var hits = await eventVectorRepo.ScrollByFilterAsync(
            limit:           CandidateCount,
            filterCategories: categoryNames,
            afterDate:        afterDate,
            ct:               ct);

        logger.LogInformation(
            "[CategoryFallback] ScrollHits={Count} for categories=[{Categories}]",
            hits.Count, string.Join(", ", categoryNames));

        if (hits.Count == 0)
            return new List<RankedEvent>();

        // Re-rank by interest score weight only (SemanticScore will be 0 from scroll)
        var categoryWeights = topScores.ToDictionary(s => s.Category, s => s.Score);
        var ranked = RecommendationRanker.Rank(
            hits, categoryWeights,
            topN: request.TopN, maxPerCategory: MaxPerCategory);

        return ranked
            .Select(r => new RankedEvent(r.EventId, r.FinalScore, r.SemanticScore, "category_fallback"))
            .ToList();
    }

    // ── Path 3: Cold-Start ────────────────────────────────────────
    // New user — no interest scores, no behavior vectors.
    // Uses ScrollByFilterAsync with globally popular categories.

    private async Task<List<RankedEvent>> TryColdStartFallbackAsync(
        GetRecommendationsQuery request,
        CancellationToken ct)
    {
        var popular   = await globalStatRepo.GetTopCategoriesAsync(ColdStartTopN,ct);
        var afterDate = request.FutureOnly ? DateTime.UtcNow : (DateTime?)null;

        logger.LogInformation(
            "[ColdStart] PopularCategories={Count}", popular.Count);

        if (popular.Count == 0)
            return new List<RankedEvent>();

        // ScrollByFilterAsync: no query vector — retrieves by popular category filter only
        var hits = await eventVectorRepo.ScrollByFilterAsync(
            limit:            request.TopN,
            filterCategories: popular.Select(c => c.Category).ToList(),
            afterDate:        afterDate,
            ct:               ct);

        logger.LogInformation(
            "[ColdStart] ScrollHits={Count}", hits.Count);

        return hits
            .Select(h => new RankedEvent(h.EventId, 0f, 0f, "popular_fallback"))
            .ToList();
    }

    private record RankedEvent(Guid EventId, float FinalScore, float SemanticScore, string Source);
}