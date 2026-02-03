
using AI.Domain.Entities;
namespace AI.Application.Abstractions;
public interface IRecommendationService 
{ 
    Task<RecommendationSet> GenerateRecommendationsAsync(Guid userId, CancellationToken cancellationToken = default); 
    Task<RecommendationSet?> GetLatestRecommendationsAsync(Guid userId, CancellationToken cancellationToken = default); 
    //Task<IReadOnlyList<RecommendationSet>> GetRecommendationHistoryAsync(Guid userId, CancellationToken cancellationToken = default);
}