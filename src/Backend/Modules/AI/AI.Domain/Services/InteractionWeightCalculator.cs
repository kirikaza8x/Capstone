using AI.Domain.Repositories;

namespace AI.Domain.Services
{
    /// <summary>
    /// Service to calculate the effective interaction weight for a user action,
    /// Check Personal Bias (High Priority)
    /// Does this specific user have a unique weight for this action?
    /// (e.g., "User X clicks too much, weight = 0.1")
    /// If yes, use it.
    /// If no, fallback to global weight.
    /// </summary>
    public class InteractionWeightCalculator
    {
        private readonly IUserWeightProfileRepository _personalRepo;
        private readonly IInteractionWeightRepository _globalRepo;

        public InteractionWeightCalculator(
            IUserWeightProfileRepository personalRepo,
            IInteractionWeightRepository globalRepo)
        {
            _personalRepo = personalRepo;
            _globalRepo = globalRepo;
        }

        public async Task<double> CalculateWeightAsync(Guid userId, string actionType)
        {

            var personalProfile = await _personalRepo.GetAsync(userId, actionType);
            if (personalProfile != null)
            {
                return personalProfile.PersonalizedWeight;
            }

            var globalWeight = await _globalRepo.GetByActionTypeAsync(actionType);

            // Default safety: If database is empty, return 1.0 to avoid breaking math
            return globalWeight?.Weight ?? 1.0;
        }
    }
}