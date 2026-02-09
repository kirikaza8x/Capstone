using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    public class UserWeightProfile : AggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }
        public string ActionType { get; private set; } = string.Empty; // e.g., "view", "click", "buy"
        
        // The specific weight for this user (e.g., 0.5 instead of 2.0)
        public double PersonalizedWeight { get; private set; }
        
        // How many times have we observed this to confirm our belief?
        public int ConfidenceCount { get; private set; } 

        private UserWeightProfile() { }

        public static UserWeightProfile Create(Guid userId, string actionType, double initialWeight)
        {
            return new UserWeightProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ActionType = actionType.ToLowerInvariant(),
                PersonalizedWeight = initialWeight,
                ConfidenceCount = 1
            };
        }

        public void AdjustWeight(double newWeight)
        {
            PersonalizedWeight = newWeight;
            ConfidenceCount++;
        }

        protected override void Apply(IDomainEvent @event)
        {
            // Implement event application logic if needed
        }
    }
}