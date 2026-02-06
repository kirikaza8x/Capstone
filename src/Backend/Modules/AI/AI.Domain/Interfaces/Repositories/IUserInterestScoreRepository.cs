using AI.Domain.Entities;
using Shared.Domain.Data;
using Shared.Domain.DDD; // Assuming your base IRepository lives here

namespace AI.Domain.Repositories
{
    
    // 4. SCORING: Read-Modify-Write
    public interface IUserInterestScoreRepository : IRepository<UserInterestScore, Guid>
    {
        /// <summary>
        /// "Get me the score for User X in Category 'Jazz'"
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        Task<UserInterestScore?> GetAsync(Guid userId, string category);
        

        /// <summary>
        /// layer (Recommendations): "Get all scores for User X"
        /// </summary>
        /// <param name="userId"></param>
        Task<List<UserInterestScore>> GetAllForUserAsync(Guid userId);
    }
}