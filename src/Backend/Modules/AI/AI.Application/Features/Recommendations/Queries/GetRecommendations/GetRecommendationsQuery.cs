using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.Recommendations.Queries;

/// <summary>
/// Semantic recommendation query — returns ranked EventIds.
/// Caller passes EventIds to Events module for display details.
/// </summary>
public sealed record GetRecommendationsQuery(
    Guid UserId,
    int  TopN       = 20,
    bool FutureOnly = true
) : IQuery<List<EventRecommendationResult>>;

/// <summary>
/// A single recommendation result.
///
/// SemanticScore — raw Qdrant cosine similarity (0–1)
/// FinalScore    — after category boost + recency boost (0–1)
/// Source        — "semantic" | "category_fallback" | "popular_fallback"
/// </summary>
public record EventRecommendationResult(
    Guid   EventId,
    float  FinalScore,
    float  SemanticScore,
    string Source
);