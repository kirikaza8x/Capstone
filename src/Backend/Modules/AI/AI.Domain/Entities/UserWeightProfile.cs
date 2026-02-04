using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    /// <summary>
    /// Stores PERSONALIZED weights for each user
    /// This is where LEARNING happens - weights adapt based on user behavior
    /// </summary>
    public class UserWeightProfile : AggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }
        public string ActionType { get; private set; } = default!;
        public double PersonalizedWeight { get; private set; }
        public int ObservationCount { get; private set; }
        public double SuccessRate { get; private set; }
        public DateTime LastUpdatedAt { get; private set; }

        private UserWeightProfile() { }

        public static UserWeightProfile Create(Guid userId, string actionType, double initialWeight)
        {
            return new UserWeightProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ActionType = actionType,
                PersonalizedWeight = initialWeight,
                ObservationCount = 0,
                SuccessRate = 0,
                LastUpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// ALGORITHM: Incremental Mean Calculation + Gradient Descent
        /// Called when action leads to conversion
        /// </summary>
        public void RecordSuccess()
        {
            ObservationCount++;
            // Incremental mean: new_mean = (old_mean * (n-1) + new_value) / n
            SuccessRate = ((SuccessRate * (ObservationCount - 1)) + 1.0) / ObservationCount;
            
            // Increase weight (gradient step)
            PersonalizedWeight = Math.Min(1.0, PersonalizedWeight + 0.05);
            LastUpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Called when action doesn't lead to conversion
        /// </summary>
        public void RecordFailure()
        {
            ObservationCount++;
            SuccessRate = (SuccessRate * (ObservationCount - 1)) / ObservationCount;
            
            // Decrease weight (gradient step)
            PersonalizedWeight = Math.Max(0.1, PersonalizedWeight - 0.02);
            LastUpdatedAt = DateTime.UtcNow;
        }

        public void AdjustWeight(double delta)
        {
            PersonalizedWeight = Math.Clamp(PersonalizedWeight + delta, 0.1, 1.0);
            LastUpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// ALGORITHM: Confidence Calculation (Thompson Sampling inspired)
        /// Higher observations = more confidence in personalized weight
        /// </summary>
        public double Confidence => Math.Min(1.0, ObservationCount / 50.0);

        protected override void Apply(IDomainEvent @event)
        {
            // Event sourcing hook
        }
    }
}