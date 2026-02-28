using AI.Application.Features.Recommendations.DTOs;
using Shared.Application.Messaging;

namespace AI.Application.Features.Recommendations.Queries
{
    public record GetRecommendationsQuery(
        Guid UserId,
        int Count = 10
    ) : IQuery<List<RecommendationResultDto>>;
}