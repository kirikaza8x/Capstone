using AI.Domain.Entities;
using Shared.Domain.Data;

namespace AI.Domain.Repositories
{
    public interface IUserInterestScoreRepository : IRepository<UserInterestScore, Guid>
    {
        Task<IReadOnlyList<UserInterestScore>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserInterestScore?> GetByUserAndCategoryAsync(Guid userId, int categoryId, CancellationToken cancellationToken = default);
    }

}
