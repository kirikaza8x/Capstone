using AI.Domain.Entities;
using Shared.Domain.Data;
namespace AI.Domain.Repositories;

public interface IRecommendationSetRepository : IRepository<RecommendationSet, Guid>
{
    Task<RecommendationSet?> GetLatestForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecommendationSet>> GetHistoryForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}