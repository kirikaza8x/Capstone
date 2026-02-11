using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    /// <summary>
    /// Defines the point value for different interaction types.
    /// SINGLETON PATTERN: Each ActionType should have exactly ONE active weight at a time.
    /// VERSIONING: Supports A/B testing by allowing multiple weights with different versions.
    /// </summary>
    public class InteractionWeight : AggregateRoot<Guid>
    {
        public string ActionType { get; private set; } = string.Empty;  // e.g., "view", "click", "purchase"
        public double Weight { get; private set; }                      // e.g., 1.0, 5.0, 25.0
        public string? Description { get; private set; }
        
        // ===== OPTIMIZATION: Support A/B testing and gradual rollouts =====
        public string Version { get; private set; } = "default";
        public DateTime? DeactivatedAt { get; private set; }

        private InteractionWeight() { }

        public static InteractionWeight Create(
            string actionType, 
            double weight, 
            string? description = null,
            string version = "default")
        {
            if (string.IsNullOrWhiteSpace(actionType))
                throw new ArgumentException("ActionType cannot be empty.", nameof(actionType));
            
            if (weight < 0) 
                throw new InvalidOperationException("Weight cannot be negative.");
            
            return new InteractionWeight
            {
                Id = Guid.NewGuid(),
                ActionType = actionType.Trim().ToLowerInvariant(),
                Weight = weight,
                Description = description,
                Version = version,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Updates the weight value. Useful for live tuning without creating new records.
        /// </summary>
        public void UpdateWeight(double newWeight, string? newDescription = null)
        {
            if (newWeight < 0) 
                throw new InvalidOperationException("Weight cannot be negative.");
            
            Weight = newWeight;
            
            if (newDescription != null)
                Description = newDescription;
        }

        /// <summary>
        /// Deactivates this weight (e.g., when rolling out a new version)
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            DeactivatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Reactivates a previously deactivated weight
        /// </summary>
        public void Reactivate()
        {
            IsActive = true;
            DeactivatedAt = null;
        }

        /// <summary>
        /// Creates a new version of this weight (useful for A/B testing)
        /// </summary>
        public static InteractionWeight CreateVariant(
            InteractionWeight baseWeight, 
            string newVersion, 
            double newWeight)
        {
            return new InteractionWeight
            {
                Id = Guid.NewGuid(),
                ActionType = baseWeight.ActionType,
                Weight = newWeight,
                Description = $"{baseWeight.Description} (Variant: {newVersion})",
                Version = newVersion,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        protected override void Apply(IDomainEvent @event)
        {
            // Implement event sourcing logic if needed
            // Example: InteractionWeightUpdated, InteractionWeightDeactivated events
        }
    }
}