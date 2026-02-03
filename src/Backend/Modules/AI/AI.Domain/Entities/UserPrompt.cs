using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    public class UserPrompt : AggregateRoot<Guid>
    {
        public string Title { get; private set; } = default!;
        public string Content { get; private set; } = default!;
        public Guid UserId { get; private set; }
        public string? Description { get; private set; }
        public Guid? EventId { get; private set; }

        private UserPrompt() { }

        public static UserPrompt Create(
            string title,
            string content,
            Guid userId,
            string? description = null,
            Guid? eventId = null)
        {
            return new UserPrompt
            {
                Id = Guid.NewGuid(),
                Title = title,
                Content = content,
                UserId = userId,
                Description = description,
                EventId = eventId
            };
        }

        // Domain behaviors
        public void UpdateContent(string newContent)
        {
            Content = newContent;
        }

        public void AttachToEvent(Guid eventId)
        {
            EventId = eventId;
        }

        public void UpdateDescription(string newDescription)
        {
            Description = newDescription;
        }
        protected override void Apply(IDomainEvent @event)
        {
            // switch (@event)
            // {
                
            // }
        }
    }
}
