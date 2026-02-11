using AI.Application.Features.Recommendations.DTOs;

namespace AI.Application.Abstractions
{
    public interface IRecommendationService
    {
        Task<List<RecommendationResultDto>> GetRecommendationsAsync(Guid? userId, int topN = 10);
    }

}