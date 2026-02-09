using AI.Domain.Entities;
using Shared.Domain.Data;

namespace AI.Domain.Repositories
{
    
    /// <summary>
    /// Repository interface for managing InteractionWeight entities.
    /// ex: global weights for different user actions (click, view, purchase).
    /// ps: These weights influence AI recommendation algorithms.
    /// CONFIGURATION: Read-Heavy, Cacheable
    /// ps:"Get me the global weight for a 'click'"
    /// </summary>
    public interface IInteractionWeightRepository : IRepository<InteractionWeight, Guid>
    {
        Task<InteractionWeight?> GetByActionTypeAsync(string actionType);
    }

    /// <summary>
    /// Repository interface for managing UserWeightProfile entities.
    /// ex: personalized weights for different user actions per user.
    /// ex: User A's weight for "click" might be 0.5, while User B's is 2.0.
    /// </summary>
    public interface IUserWeightProfileRepository : IRepository<UserWeightProfile,Guid>
    {
        Task<UserWeightProfile?> GetAsync(Guid userId, string actionType);
    }
    
}