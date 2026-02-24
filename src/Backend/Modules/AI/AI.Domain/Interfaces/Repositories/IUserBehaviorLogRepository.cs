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
        /// <summary>
        /// Fetches all raw logs since a specific time (e.g., Last 24 Hours).
        /// We need the raw logs because the "Category" is inside the Metadata JSON,
        /// so we must process the grouping in C# memory, not SQL.
        /// </summary>
        Task<List<UserBehaviorLog>> GetLogsSinceAsync(DateTime since);
    }


}