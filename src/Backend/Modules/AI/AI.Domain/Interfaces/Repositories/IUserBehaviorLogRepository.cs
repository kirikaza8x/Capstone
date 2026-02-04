using AI.Domain.Entities;
using Shared.Domain.Data;

namespace AI.Domain.Repositories
{
    public interface IUserBehaviorLogRepository : IRepository<UserBehaviorLog, Guid>
    {
        Task<List<UserBehaviorLog>> GetByUserAsync(Guid userId, CancellationToken cancellationToken);
        Task<List<UserBehaviorLog>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken);
        Task<List<UserBehaviorLog>> GetFollowUpActionsAsync(
            Guid userId, 
            Guid eventId, 
            DateTime afterTimestamp, 
            CancellationToken cancellationToken);
        Task<int> GetTotalCountAsync(CancellationToken cancellationToken);
    }
}