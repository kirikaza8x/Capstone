using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    public class InteractionWeight : AggregateRoot<Guid>
    {
        public string ActionType { get; private set; } = default!;
        public double Weight { get; private set; }
        public string? Description { get; private set; }

        private InteractionWeight() { }

        public static InteractionWeight Create(string actionType, double weight, string? description = null)
        {
            if (weight < 0)
                throw new InvalidOperationException("Weight cannot be negative.");
            return new InteractionWeight
            {
                Id = Guid.NewGuid(),
                ActionType = actionType,
                Weight = weight,
                Description = description,
                IsActive = true
            };
        }

        public void UpdateWeight(double newWeight)
        {
            Weight = newWeight;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }

        protected override void Apply(IDomainEvent @event)
        {
            // Event sourcing hook if needed
        }
    }
}