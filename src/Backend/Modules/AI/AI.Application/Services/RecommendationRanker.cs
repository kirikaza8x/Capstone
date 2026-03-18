using AI.Application.Abstractions.Qdrant.Model;

namespace AI.Application.Features.Recommendations.Services;

/// <summary>
/// In-memory re-ranking applied AFTER Qdrant returns cosine similarity results.
///
/// WHY: Qdrant only knows semantic similarity. It doesn't know:
///   - How soon the event starts (recency)
///   - How strongly the user has converted on this category (intent strength)
///   - Whether results are diverse enough (variety)
///
/// PIPELINE (applied in order):
///   1. Category weight boost  — intent strength from UserInterestScore
///   2. Recency boost          — events starting soon rank higher
///   3. Re-sort by final score
///   4. Diversity filter       — cap results per category
///   5. Take topN
/// </summary>
public static class RecommendationRanker
{
    /// <summary>
    /// Re-rank Qdrant results using additional signals.
    /// </summary>
    /// <param name="hits">Raw Qdrant results ordered by cosine similarity.</param>
    /// <param name="categoryWeights">UserInterestScore per category — higher = stronger intent.</param>
    /// <param name="topN">Final result count.</param>
    /// <param name="maxPerCategory">Max events per category for diversity.</param>
    /// <param name="recencyWindowDays">Events starting within this window get a recency boost.</param>
    public static List<RankedEventResult> Rank(
        IReadOnlyList<EventSearchResult> hits,
        IReadOnlyDictionary<string, double> categoryWeights,
        int    topN             = 20,
        int    maxPerCategory   = 3,
        int    recencyWindowDays = 30)
    {
        var now = DateTime.UtcNow;

        // ── Step 1+2: Score each hit ──────────────────────────────
        var scored = hits.Select(hit =>
        {
            var baseScore = (double)hit.Score;

            // Category weight boost — average interest score across event's categories
            // Normalised to [0, 1] assuming max interest score ~25 (purchase weight)
            var categoryBoost = hit.Categories.Count > 0
                ? hit.Categories
                    .Select(c => categoryWeights.TryGetValue(c, out var w) ? w : 0.0)
                    .Average() / 25.0
                : 0.0;

            // Recency boost — linear decay from 1.0 (today) to 0.0 (recencyWindowDays out)
            // Events beyond the window get 0 boost (not penalised, just not boosted)
            var daysUntilStart = (hit.EventStartAt - now).TotalDays;
            var recencyBoost = daysUntilStart is >= 0 and <= 30
                ? 1.0 - (daysUntilStart / recencyWindowDays)
                : 0.0;

            // Final score — weights tuned to keep semantic similarity dominant
            // 60% semantic, 25% intent, 15% recency
            var finalScore = (baseScore * 0.60)
                           + (categoryBoost * 0.25)
                           + (recencyBoost  * 0.15);

            return new RankedEventResult(
                EventId:       hit.EventId,
                SemanticScore: hit.Score,
                FinalScore:    (float)finalScore,
                Categories:    hit.Categories,
                EventStartAt:  hit.EventStartAt
            );
        })
        .OrderByDescending(r => r.FinalScore)
        .ToList();

        // ── Step 3: Diversity filter ──────────────────────────────
        // Cap results per category — walk ranked list, track category counts
        var categoryCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var diverse        = new List<RankedEventResult>(topN);

        foreach (var result in scored)
        {
            if (diverse.Count >= topN) break;

            // Check if any of this event's categories has hit the cap
            bool capped = result.Categories.Any(c =>
                categoryCounts.TryGetValue(c, out var count) && count >= maxPerCategory);

            if (capped) continue;

            diverse.Add(result);

            foreach (var cat in result.Categories)
                categoryCounts[cat] = categoryCounts.TryGetValue(cat, out var n) ? n + 1 : 1;
        }

        return diverse;
    }
}

/// <summary>Re-ranked event result with all scoring signals visible.</summary>
public record RankedEventResult(
    Guid         EventId,
    float        SemanticScore,  // raw Qdrant cosine similarity
    float        FinalScore,     // after boosts
    List<string> Categories,
    DateTime     EventStartAt
);