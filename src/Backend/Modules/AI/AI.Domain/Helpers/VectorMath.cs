namespace AI.Domain.Helpers;

/// <summary>
/// Pure vector math utilities — no domain knowledge, no dependencies.
///
/// Used by:
///   - UserEmbedding.Recalculate()  → L2Normalize, WeightedCentroid
///   - Qdrant repos                 → L2Normalize before upsert if embedding model doesn't guarantee unit vectors
///   - Recommendation pipeline      → CosineSimilarity, DotProduct for in-memory ranking
///
/// ALL methods are stateless and allocation-minimal where possible.
/// </summary>
public static class VectorMath
{
    // ── Normalization ─────────────────────────────────────────────

    /// <summary>
    /// Returns an L2-normalized (unit length) copy of the vector.
    /// If the vector is already unit length (within 1e-6), returns the original — no alloc.
    /// If the vector is near-zero (norm less than 1e-8), returns the original unchanged to avoid NaN.
    /// </summary>
    public static float[] L2Normalize(float[] vector)
    {
        if (vector is null || vector.Length == 0)
            throw new ArgumentException("Vector cannot be null or empty.", nameof(vector));

        double sumOfSquares = 0;
        for (int i = 0; i < vector.Length; i++)
            sumOfSquares += (double)vector[i] * vector[i];

        double norm = Math.Sqrt(sumOfSquares);

        // Already unit — skip allocation
        if (Math.Abs(norm - 1.0) < 1e-6) return vector;

        // Near-zero vector — normalizing would produce NaN, return as-is
        if (norm < 1e-8) return vector;

        var result = new float[vector.Length];
        for (int i = 0; i < vector.Length; i++)
            result[i] = (float)(vector[i] / norm);

        return result;
    }

    /// <summary>
    /// Normalizes in-place — mutates the original array.
    /// Use when you own the array and want to avoid the extra allocation.
    /// </summary>
    public static void L2NormalizeInPlace(float[] vector)
    {
        if (vector is null || vector.Length == 0)
            throw new ArgumentException("Vector cannot be null or empty.", nameof(vector));

        double sumOfSquares = 0;
        for (int i = 0; i < vector.Length; i++)
            sumOfSquares += (double)vector[i] * vector[i];

        double norm = Math.Sqrt(sumOfSquares);

        if (Math.Abs(norm - 1.0) < 1e-6 || norm < 1e-8) return;

        for (int i = 0; i < vector.Length; i++)
            vector[i] = (float)(vector[i] / norm);
    }

    // ── Similarity ────────────────────────────────────────────────

    /// <summary>
    /// Dot product of two vectors.
    /// When both vectors are L2-normalized, this equals cosine similarity.
    /// Throws if dimensions differ.
    /// </summary>
    public static double DotProduct(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException(
                $"Vector dimensions must match: {a.Length} vs {b.Length}.");

        double sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }

    /// <summary>
    /// Cosine similarity between two vectors (handles non-normalized inputs).
    /// Returns value in [-1, 1]. Returns 0 if either vector is near-zero.
    /// For pre-normalized vectors, prefer DotProduct — it's faster.
    /// </summary>
    public static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException(
                $"Vector dimensions must match: {a.Length} vs {b.Length}.");

        double dot = 0, normA = 0, normB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot   += a[i] * b[i];
            normA += (double)a[i] * a[i];
            normB += (double)b[i] * b[i];
        }

        double denom = Math.Sqrt(normA) * Math.Sqrt(normB);
        return denom < 1e-8 ? 0 : dot / denom;
    }

    // ── Aggregation ───────────────────────────────────────────────

    /// <summary>
    /// Weighted centroid of multiple vectors — used to build a user interest vector
    /// from their category embeddings weighted by UserInterestScore.
    ///
    /// Each input vector is L2-normalized before weighting so magnitude
    /// differences between embeddings don't skew the result.
    ///
    /// The result is L2-normalized before returning, making it ready for
    /// direct cosine similarity via dot product in Qdrant or in-memory.
    ///
    /// Example:
    ///   vectors  = { "music": [0.1, 0.9, ...], "sport": [0.8, 0.2, ...] }
    ///   weights  = { "music": 3.5, "sport": 1.2 }
    ///   → weighted average → L2-normalize → user interest vector
    /// </summary>
    public static float[] WeightedCentroid(
        IEnumerable<(float[] Vector, double Weight)> items)
    {
        float[]? centroid = null;
        double totalWeight = 0;

        foreach (var (vector, weight) in items)
        {
            double w = Math.Max(weight, 0); // ignore negative weights
            if (w == 0) continue;

            float[] normalized = L2Normalize(vector);

            if (centroid is null)
            {
                centroid = new float[normalized.Length];
            }
            else if (centroid.Length != normalized.Length)
            {
                throw new ArgumentException(
                    $"All vectors must have the same dimension. " +
                    $"Expected {centroid.Length}, got {normalized.Length}.");
            }

            for (int i = 0; i < centroid.Length; i++)
                centroid[i] += (float)(normalized[i] * w);

            totalWeight += w;
        }

        if (centroid is null || totalWeight == 0)
            throw new InvalidOperationException(
                "Cannot compute centroid — no valid vectors with positive weight.");

        // Normalize total
        for (int i = 0; i < centroid.Length; i++)
            centroid[i] /= (float)totalWeight;

        return L2Normalize(centroid);
    }

    // ── Validation ────────────────────────────────────────────────

    /// <summary>
    /// Returns true if all vectors in the collection share the same dimension.
    /// Use before passing to WeightedCentroid if you want an early check.
    /// </summary>
    public static bool AreSameDimension(IEnumerable<float[]> vectors)
    {
        int? expected = null;
        foreach (var v in vectors)
        {
            if (expected is null) { expected = v.Length; continue; }
            if (v.Length != expected) return false;
        }
        return true;
    }

    /// <summary>
    /// Returns the L2 norm (magnitude) of a vector.
    /// Useful for checking if an embedding is already normalized.
    /// </summary>
    public static double Norm(float[] vector)
    {
        double sum = 0;
        for (int i = 0; i < vector.Length; i++)
            sum += (double)vector[i] * vector[i];
        return Math.Sqrt(sum);
    }
}