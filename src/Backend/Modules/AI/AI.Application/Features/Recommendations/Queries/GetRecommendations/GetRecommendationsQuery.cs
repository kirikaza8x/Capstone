using AI.Application.Features.Recommendations.DTOs;
using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.Recommendations.Queries
{
    public record GetRecommendationsQuery(
        Guid UserId,
        int Count = 10
    ) : IQuery<Guid>;

    // public record GetRecommendationsProtoQuery(
    //     Guid UserId,
    //     IEnumerable<string> categoryNames,
    //     IEnumerable<string> hashtagNames
    // );

    // public sealed record GetRecommendationsProtoQuery(Guid UserId)
    // : IQuery<List<RecommendationResulLiteDto>>;

    public sealed record GetRecommendationsProtoQuery(
    Guid UserId,
    int TopN = 10  
) : IQuery<List<RecommendationResultLiteDto>>;
}