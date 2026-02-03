using AI.Domain.Entities;
using Shared.Domain.Data;

namespace AI.Domain.Repositories
{
    public interface IInteractionWeightRepository : IRepository<InteractionWeight, Guid>
    {
        Task<InteractionWeight?> GetByActionTypeAsync(string actionType, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<InteractionWeight>> GetActiveWeightsAsync(CancellationToken cancellationToken = default);
    }

    
    
}
