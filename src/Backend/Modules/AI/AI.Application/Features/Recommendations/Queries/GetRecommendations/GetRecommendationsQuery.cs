using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.Recommendations.Queries;

/// <summary>
/// Returns ranked events with mini display details — enough for a recommendation card.
/// Frontend can render the list immediately without a second call.
/// Clicking an event uses EventId to call the full event detail endpoint.
/// </summary>
public sealed record GetRecommendationsQuery(
    Guid UserId,
    int  TopN       = 20,
    bool FutureOnly = false
) : IQuery<List<EventRecommendationResult>>;

/// <summary>
/// A single recommendation result — ranking scores + mini display data.
///
/// FinalScore    — after category boost + recency boost (0–1)
/// SemanticScore — raw Qdrant cosine similarity (0–1)
/// Source        — "semantic" | "category_fallback" | "popular_fallback"
/// </summary>
public record EventRecommendationResult(
    Guid      EventId,
    float     FinalScore,
    float     SemanticScore,
    string    Source,
    // ── Mini display data ─────────────────────────────────────────
    string    Title,
    string?   BannerUrl,
    string?   Location,
    DateTime? EventStartAt,
    DateTime? EventEndAt,
    decimal?  MinPrice,
    decimal?  MaxPrice
);