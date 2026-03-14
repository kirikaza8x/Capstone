using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    /// <summary>
    /// Defines the point value assigned to a specific interaction type.
    ///
    /// SINGLETON INVARIANT: exactly one active record per (ActionType, Version) pair.
    /// Enforced at the DB level with a unique index on (ActionType, Version, IsActive=true).
    ///
    /// VERSIONING: Use Version to run A/B tests. Deactivate the old version before
    /// activating the new one to prevent double-counting.
    /// </summary>
    public class InteractionWeight : AggregateRoot<Guid>
    {
        public string ActionType { get; private set; } = string.Empty;
        public double Weight { get; private set; }
        public string? Description { get; private set; }
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
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentException("Version cannot be empty.", nameof(version));
            if (weight < 0)
                throw new ArgumentException("Weight cannot be negative.", nameof(weight));

            return new InteractionWeight
            {
                Id = Guid.NewGuid(),
                ActionType = actionType.Trim().ToLowerInvariant(),
                Weight = weight,
                Description = description?.Trim(),
                Version = version.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a variant of an existing weight for A/B testing.
        /// Delegates to Create() so all validation is applied consistently.
        /// </summary>
        public static InteractionWeight CreateVariant(
            InteractionWeight baseWeight,
            string newVersion,
            double newWeight)
        {
            if (baseWeight is null)
                throw new ArgumentNullException(nameof(baseWeight));

            return Create(
                actionType: baseWeight.ActionType,
                weight: newWeight,
                description: $"{baseWeight.Description} (variant: {newVersion})",
                version: newVersion);
        }

        /// <summary>
        /// Updates the weight value in place. Useful for live tuning without creating a new record.
        /// </summary>
        public void UpdateWeight(double newWeight, string? newDescription = null)
        {
            if (newWeight < 0)
                throw new ArgumentException("Weight cannot be negative.", nameof(newWeight));

            Weight = newWeight;

            if (newDescription is not null)
                Description = newDescription.Trim();
        }

        /// <summary>
        /// Deactivates this weight. Raise before activating the replacement to avoid overlap.
        /// </summary>
        public void Deactivate()
        {
            if (!IsActive)
                throw new InvalidOperationException(
                    $"InteractionWeight '{ActionType}' v'{Version}' is already inactive.");

            IsActive = false;
            DeactivatedAt = DateTime.UtcNow;

            // RaiseDomainEvent(new InteractionWeightDeactivatedEvent(
            //     WeightId:      Id,
            //     ActionType:    ActionType,
            //     Version:       Version,
            //     DeactivatedAt: DeactivatedAt.Value));
        }

        public void Reactivate()
        {
            IsActive = true;
            DeactivatedAt = null;
        }

        protected override void Apply(IDomainEvent @event)
        {
            // switch (@event)
            // {
            //     case InteractionWeightDeactivatedEvent e:
            //         IsActive      = false;
            //         DeactivatedAt = e.DeactivatedAt;
            //         break;
            // }
        }
    }
}