using AI.Domain.Entities;
using Shared.Domain.Data;

namespace AI.Domain.Repositories
{
    public interface IUserWeightProfileRepository : IRepository<UserWeightProfile, Guid>
    {
        Task<UserWeightProfile?> GetByUserAndActionAsync(
            Guid userId, 
            string actionType, 
            CancellationToken cancellationToken);
        Task<List<UserWeightProfile>> GetByUserAsync(Guid userId, CancellationToken cancellationToken);
    }    
}