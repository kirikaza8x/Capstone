using AI.Domain.Entities;
using Shared.Domain.Data;

namespace AI.Domain.Repositories
{
    /// <summary>
    /// Repository interface for managing UserBehaviorLog entities.
    /// The Orchestrator primarily just "Adds" logs.
    /// Queries are usually for Background Jobs (e.g., "Get logs from last 24h").
    /// </summary>
    public interface IUserBehaviorLogRepository : IRepository<UserBehaviorLog, Guid>
    {
        
    }

    
}