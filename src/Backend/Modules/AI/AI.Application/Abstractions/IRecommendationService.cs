using AI.Application.Features.Recommendations.DTOs;
using Events.PublicApi.Records;

namespace AI.Application.Abstractions
{
    public interface IRecommendationService
    {
        Task<List<RecommendationResultDto>> GetRecommendationsAsync(Guid? userId, int topN = 10);

    }
    public interface IRecommendationAiService
    {
        Task<IReadOnlyList<int>> RankEventsAsync(
            IReadOnlyList<EventRecommendationFeature> candidates,
            CancellationToken cancellationToken = default);
    }


}