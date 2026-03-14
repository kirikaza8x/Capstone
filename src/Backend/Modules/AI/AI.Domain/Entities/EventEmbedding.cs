// AI.Domain/Entities/EventEmbedding.cs
using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    /// <summary>
    /// Stores the pgvector embedding for a published event.
    /// One row per event. Replaced entirely on re-embed.
    /// </summary>
    public class EventEmbedding : AggregateRoot<Guid>
    {
        public Guid EventId { get; private set; }
        public float[] Embedding { get; private set; } = default!;
        public string ModelName { get; private set; } = default!;
        public DateTime EmbeddedAt { get; private set; }

        private EventEmbedding() { }

        public static EventEmbedding Create(
            Guid eventId,
            float[] embedding,
            string modelName = "all-MiniLM-L6-v2")
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("EventId cannot be empty.", nameof(eventId));
            if (embedding is null || embedding.Length == 0)
                throw new ArgumentException("Embedding cannot be empty.", nameof(embedding));

            var now = DateTime.UtcNow;

            return new EventEmbedding
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Embedding = embedding,
                ModelName = modelName,
                EmbeddedAt = now,
                CreatedAt = now
            };
        }

        /// <summary>
        /// Full replace — called when event content changes and needs re-embedding.
        /// </summary>
        public void Update(float[] newEmbedding)
        {
            if (newEmbedding is null || newEmbedding.Length == 0)
                throw new ArgumentException("Embedding cannot be empty.", nameof(newEmbedding));

            Embedding = newEmbedding;
            EmbeddedAt = DateTime.UtcNow;
        }

        protected override void Apply(IDomainEvent @event) { }
    }
}