using Shared.Domain.DDD;

namespace Users.Domain.Entities
{
    public class Role : AggregateRoot<Guid>
    {
        public string Name { get; private set; } = default!;
        public string? Description { get; private set; } = default!;

        private Role() { }

        public static Role Create(string name, string? description)
        {
            return new Role
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description
            };
        }

        public void Update(string name, string? description)
        {
            Name = name;
            Description = description ?? string.Empty;
        }

        public void Rename(string name)
        {
            Name = name;
        }

        public void ChangeDescription(string? description)
        {
            Description = description ?? string.Empty;
        }

        protected override void Apply(IDomainEvent @event)
        {
            // No-op since events are turned off
        }
    }
}
