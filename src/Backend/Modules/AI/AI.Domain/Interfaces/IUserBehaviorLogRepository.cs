using AI.Domain.Entities;
using Shared.Domain.Data;

public interface IUserBehaviorLogRepository : IRepository<UserBehaviorLog, Guid>
{
    Task<IReadOnlyList<UserBehaviorLog>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserBehaviorLog>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserBehaviorLog>> GetBySessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserBehaviorLog>> GetRecentAsync(Guid userId, int limit, CancellationToken cancellationToken = default);
}