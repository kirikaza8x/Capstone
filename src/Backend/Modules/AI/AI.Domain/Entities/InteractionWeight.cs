using Shared.Domain.DDD; // Assuming your base classes

namespace AI.Domain.Entities
{
    public class InteractionWeight : AggregateRoot<Guid>
    {
        public string ActionType { get; private set; }  = string.Empty;// e.g., "view", "click", "buy"
        public double Weight { get; private set; }     // e.g., 1.0, 2.0, 10.0
        public string? Description { get; private set; }

        private InteractionWeight() { }

        public static InteractionWeight Create(string actionType, double weight, string? description = null)
        {
            if (weight < 0) throw new InvalidOperationException("Weight cannot be negative.");
            
            return new InteractionWeight
            {
                Id = Guid.NewGuid(),
                ActionType = actionType.ToLowerInvariant(),
                Weight = weight,
                Description = description
            };
        }

        public void UpdateWeight(double newWeight)
        {
            if (newWeight < 0) throw new InvalidOperationException("Weight cannot be negative.");
            Weight = newWeight;
        }

        protected override void Apply(IDomainEvent @event)
        {
            // Implement event application logic if needed
        }
    }
}