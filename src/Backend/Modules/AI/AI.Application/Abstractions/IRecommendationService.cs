using AI.Application.Features.Recommendations.DTOs;

namespace AI.Application.Services
{
    public interface IRecommendationService
    {
        Task<List<RecommendationResult>> GetRecommendationsAsync(Guid? userId, int topN = 10);
    }

}