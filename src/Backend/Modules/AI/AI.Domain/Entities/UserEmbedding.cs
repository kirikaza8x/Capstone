// using Shared.Domain.DDD;

// namespace AI.Domain.Entities
// {
//     /// <summary>
//     /// Represents a user's preference as a vector in embedding space.
//     /// Built by taking a weighted average of the category embeddings the user has interacted with,
//     /// weighted by their current (decayed) UserInterestScore for each category.
//     ///
//     /// NORMALIZATION CONTRACT: the stored Embedding is always L2-normalized (unit length).
//     /// This enables direct cosine similarity via dot product with no extra division.
//     ///
//     /// REBUILD TRIGGER: mark stale via a flag when new UserBehaviorLogs arrive;
//     /// a background job calls Recalculate() on all stale records.
//     /// </summary>
//     public class UserEmbedding : AggregateRoot<Guid>
//     {
//         private const int DefaultDimension = 384;
//         private const double ConfidenceHalfLifeDays = 7.0;
//         private const double ConfidenceScaleFactor = 0.5;

//         public Guid UserId { get; private set; }
//         public float[] Embedding { get; private set; } = default!;
//         public int Dimension { get; private set; }
//         public int InteractionCount { get; private set; }
//         public double Confidence { get; private set; }
//         public DateTime LastCalculated { get; private set; }
//         public bool IsStale { get; private set; }

//         private readonly List<string> _contributingCategories = new();
//         public IReadOnlyCollection<string> ContributingCategories =>
//             _contributingCategories.AsReadOnly();

//         private UserEmbedding() { }

//         public static UserEmbedding Create(
//             Guid userId,
//             float[] embedding,
//             List<string> contributingCategories)
//         {
//             if (userId == Guid.Empty)
//                 throw new ArgumentException("UserId cannot be empty.", nameof(userId));
//             if (embedding is null || embedding.Length == 0)
//                 throw new ArgumentException("Embedding cannot be null or empty.", nameof(embedding));
//             ArgumentNullException.ThrowIfNull(contributingCategories);
//             if (contributingCategories.Count == 0)
//                 throw new ArgumentException("Contributing categories cannot be empty.", nameof(contributingCategories));

//             var now = DateTime.UtcNow;

//             var entity = new UserEmbedding
//             {
//                 Id = Guid.NewGuid(),
//                 UserId = userId,
//                 Embedding = L2Normalize(embedding),
//                 Dimension = embedding.Length,
//                 InteractionCount = contributingCategories.Count,
//                 LastCalculated = now,
//                 CreatedAt = now,
//                 IsStale = false,
//                 Confidence = CalculateConfidence(contributingCategories.Count, daysElapsed: 0)
//             };

//             entity._contributingCategories.AddRange(contributingCategories);
//             return entity;
//         }

//         /// <summary>
//         /// Recomputes the embedding as a weighted centroid of category embeddings.
//         /// Each input embedding is L2-normalized before weighting so magnitude differences
//         /// don't skew the result.
//         /// </summary>
//         /// <param name="categoryEmbeddings">Dictionary of category names to their embedding vectors.</param>
//         /// <param name="weights">Dictionary of category names to their weights.</param>
//         /// <exception cref="ArgumentException">Thrown when category embeddings are empty or have inconsistent dimensions.</exception>
//         public void Recalculate(
//             Dictionary<string, float[]> categoryEmbeddings,
//             Dictionary<string, double> weights)
//         {
//             if (categoryEmbeddings is null || categoryEmbeddings.Count == 0)
//                 throw new ArgumentException(
//                     "Category embeddings cannot be empty.", nameof(categoryEmbeddings));

//             int expectedDim = categoryEmbeddings.Values.First().Length;

//             if (categoryEmbeddings.Values.Any(e => e.Length != expectedDim))
//                 throw new ArgumentException(
//                     "All category embeddings must have the same dimension.",
//                     nameof(categoryEmbeddings));

//             var centroid = new float[expectedDim];
//             double totalWeight = 0;

//             foreach (var (category, rawEmbedding) in categoryEmbeddings)
//             {
//                 float[] normalized = L2Normalize(rawEmbedding);
//                 double weight = weights.TryGetValue(category, out var w) ? Math.Max(w, 0) : 1.0;

//                 for (int i = 0; i < expectedDim; i++)
//                     centroid[i] += (float)(normalized[i] * weight);

//                 totalWeight += weight;
//             }

//             if (totalWeight > 0)
//                 for (int i = 0; i < expectedDim; i++)
//                     centroid[i] /= (float)totalWeight;

//             double daysElapsed = (DateTime.UtcNow - LastCalculated).TotalDays;

//             Embedding = L2Normalize(centroid);
//             Dimension = expectedDim;
//             IsStale = false;
//             LastCalculated = DateTime.UtcNow;
//             InteractionCount = categoryEmbeddings.Count;
//             Confidence = CalculateConfidence(categoryEmbeddings.Count, daysElapsed);

//             _contributingCategories.Clear();
//             _contributingCategories.AddRange(categoryEmbeddings.Keys);
//         }

//         /// <summary>
//         /// Marks this embedding as needing a rebuild.
//         /// Called whenever new UserBehaviorLogs arrive for this user.
//         /// </summary>
//         public void MarkStale() => IsStale = true;

//         /// <summary>
//         /// Direct vector update — used by incremental online learning pipelines.
//         /// Caller is responsible for supplying a unit-normalized vector.
//         /// </summary>
//         public void UpdateEmbedding(float[] newEmbedding)
//         {
//             if (newEmbedding is null || newEmbedding.Length == 0)
//                 throw new ArgumentException(
//                     "Embedding cannot be null or empty.", nameof(newEmbedding));

//             Embedding = L2Normalize(newEmbedding);
//             Dimension = newEmbedding.Length;
//             LastCalculated = DateTime.UtcNow;
//             IsStale = false;
//         }

//         /// <summary>
//         /// Updates this instance's state from another UserEmbedding.
//         /// Used by repositories for upsert scenarios.
//         /// Respects encapsulation: only the entity can set its private properties.
//         /// </summary>
//         /// <param name="source">The source embedding to copy state from.</param>
//         public void UpdateFrom(UserEmbedding source)
//         {
//             if (source == null) throw new ArgumentNullException(nameof(source));
//             if (source.UserId != this.UserId)
//                 throw new ArgumentException("Cannot update from embedding with different UserId", nameof(source));

//             // Update vector (uses existing public method)
//             UpdateEmbedding(source.Embedding);

//             // Update other properties (allowed: we're inside the class)
//             InteractionCount = source.InteractionCount;
//             Confidence = source.Confidence;
//             LastCalculated = source.LastCalculated;
//             IsStale = source.IsStale;

//             // Update categories
//             _contributingCategories.Clear();
//             foreach (var cat in source.ContributingCategories)
//             {
//                 _contributingCategories.Add(cat);
//             }
//         }

//         /// <summary>
//         /// Adds a contributing category to the embedding.
//         /// Used when rebuilding embeddings incrementally.
//         /// </summary>
//         public void AddContributingCategory(string category)
//         {
//             if (!_contributingCategories.Contains(category))
//                 _contributingCategories.Add(category);
//         }

//         /// <summary>
//         /// Cosine similarity against another user embedding.
//         /// Because both vectors are unit-normalized, this is simply the dot product.
//         /// </summary>
//         public double CosineSimilarity(UserEmbedding? other)
//         {
//             if (other is null || other.Dimension != Dimension)
//                 return 0;

//             return DotProduct(Embedding, other.Embedding, Dimension);
//         }

//         /// <summary>
//         /// Cosine similarity against a raw category embedding (not required to be normalized).
//         /// </summary>
//         public double CosineSimilarity(float[]? categoryEmbedding)
//         {
//             if (categoryEmbedding is null || categoryEmbedding.Length != Dimension)
//                 return 0;

//             float[] normalized = L2Normalize(categoryEmbedding);
//             return DotProduct(Embedding, normalized, Dimension);
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

//         /// <summary>
//         /// Confidence is a sigmoid over interaction count, time-decayed by a configurable half-life.
//         /// </summary>
//         /// <param name="interactionCount">Number of interactions contributing to the embedding.</param>
//         /// <param name="daysElapsed">Days since the last calculation.</param>
//         /// <returns>Confidence score between 0 and 1.</returns>
//         private static double CalculateConfidence(int interactionCount, double daysElapsed)
//         {
//             double countFactor = 1.0 / (1.0 + Math.Exp(-ConfidenceScaleFactor * (interactionCount - 10)));
//             double timeFactor = Math.Pow(0.5, daysElapsed / ConfidenceHalfLifeDays);
//             return Math.Min(1.0, countFactor * timeFactor);
//         }

//         protected override void Apply(IDomainEvent @event)
//         {
//             // switch (@event)
//             // {
//             //     case UserEmbeddingRecalculatedEvent e:
//             //         Confidence     = e.Confidence;
//             //         LastCalculated = e.CalculatedAt;
//             //         IsStale        = false;
//             //         break;
//             // }
//         }
//     }
// }