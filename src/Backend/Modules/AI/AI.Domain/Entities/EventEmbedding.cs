// // AI.Domain/Entities/EventEmbedding.cs
// using Shared.Domain.DDD;

// namespace AI.Domain.Entities
// {
//     /// <summary>
//     /// Stores the pgvector embedding for a published event.
//     /// One row per event. Replaced entirely on re-embed.
//     /// </summary>
//     public class EventEmbedding : AggregateRoot<Guid>
//     {
//         public Guid EventId { get; private set; }
//         public float[] Embedding { get; private set; } = default!;
//         public string ModelName { get; private set; } = default!;
//         public DateTime EmbeddedAt { get; private set; }

//         private EventEmbedding() { }

//         public static EventEmbedding Create(
//             Guid eventId,
//             float[] embedding,
//             string modelName = "all-MiniLM-L6-v2")
//         {
//             if (eventId == Guid.Empty)
//                 throw new ArgumentException("EventId cannot be empty.", nameof(eventId));
//             if (embedding is null || embedding.Length == 0)
//                 throw new ArgumentException("Embedding cannot be empty.", nameof(embedding));

//             var now = DateTime.UtcNow;

//             return new EventEmbedding
//             {
//                 Id = Guid.NewGuid(),
//                 EventId = eventId,
//                 Embedding = L2Normalize(embedding),
//                 ModelName = modelName,
//                 EmbeddedAt = now,
//                 CreatedAt = now
//             };
//         }

//         /// <summary>
//         /// Full replace — called when event content changes and needs re-embedding.
//         /// </summary>
//         public void Update(float[] newEmbedding)
//         {
//             if (newEmbedding is null || newEmbedding.Length == 0)
//                 throw new ArgumentException("Embedding cannot be empty.", nameof(newEmbedding));

//             Embedding = newEmbedding;
//             EmbeddedAt = DateTime.UtcNow;
//         }

//         private static float[] L2Normalize(float[] vector)
//         {
//             // 1. Calculate the squared norm first
//             double sum = 0;
//             for (int i = 0; i < vector.Length; i++)
//                 sum += (double)vector[i] * vector[i];

//             double norm = Math.Sqrt(sum);

//             // OPTIMIZATION: If it's already unit length (1.0), just return the original array.
//             // 1e-6 is a safe epsilon for float precision.
//             if (Math.Abs(norm - 1.0) < 1e-6)
//                 return vector;

//             if (norm < 1e-8) return vector; // Avoid division by zero

//             var result = new float[vector.Length];
//             for (int i = 0; i < vector.Length; i++)
//                 result[i] = (float)(vector[i] / norm);

//             return result;
//         }

//         private static double DotProduct(float[] a, float[] b, int length)
//         {
//             double sum = 0;
//             for (int i = 0; i < length; i++)
//                 sum += a[i] * b[i];
//             return sum;
//         }

//         protected override void Apply(IDomainEvent @event) { }
//     }
// }