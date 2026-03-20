namespace Shared.Domain.Helpers;

/// <summary>
/// Calculates confidence scores for user embeddings.
///
/// Confidence answers: "how much should we trust this embedding?"
///   - Few interactions → low confidence (user is new or cold-start)
///   - Many interactions but old → decays over time
///   - Many recent interactions → high confidence
///
/// Formula: sigmoid(interactionCount) × time_decay_factor
///   Result is always in [0, 1].
///
/// Tune via:
///   HalfLifeDays     — how quickly confidence decays without new activity (default: 7 days)
///   ScaleFactor      — steepness of the sigmoid around the inflection point (default: 0.5)
///   InflectionPoint  — interaction count at which confidence reaches ~0.5 (default: 10)
/// </summary>
public static class EmbeddingConfidence
{
    public const double DefaultHalfLifeDays = 7.0;
    public const double DefaultScaleFactor = 0.5;
    public const int DefaultInflectionPoint = 10;

    /// <summary>
    /// Calculate confidence given interaction count and days since last calculation.
    /// Uses default tuning constants.
    /// </summary>
    public static double Calculate(int interactionCount, double daysElapsed)
        => Calculate(interactionCount, daysElapsed,
            DefaultHalfLifeDays, DefaultScaleFactor, DefaultInflectionPoint);

    /// <summary>
    /// Calculate confidence with explicit tuning parameters.
    /// Use this overload for A/B testing different decay curves.
    /// </summary>
    public static double Calculate(
        int interactionCount,
        double daysElapsed,
        double halfLifeDays,
        double scaleFactor,
        int inflectionPoint)
    {
        if (halfLifeDays <= 0)
            throw new ArgumentException("Half-life must be positive.", nameof(halfLifeDays));

        // Sigmoid over interaction count — approaches 1 as interactions grow
        double countFactor = 1.0 / (1.0 + Math.Exp(-scaleFactor * (interactionCount - inflectionPoint)));

        // Exponential time decay — halves every halfLifeDays with no new activity
        double timeFactor = Math.Pow(0.5, daysElapsed / halfLifeDays);

        return Math.Min(1.0, countFactor * timeFactor);
    }

    /// <summary>
    /// True when confidence is high enough to use the embedding for recommendation.
    /// Below this threshold, fall back to popularity-based or category-filtered results.
    /// </summary>
    public static bool IsReliable(double confidence, double threshold = 0.3)
        => confidence >= threshold;
}
