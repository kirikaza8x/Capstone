namespace AI.Application.Helpers;

/// <summary>
/// Builds the plain text string that gets passed to the embedding model.
///
/// Centralising this means:
///   - Event and behavior handlers both produce text the same way
///   - Easy to tune what fields matter for semantic similarity
///   - Unit-testable without touching infrastructure
///
/// RULE: more signal is better — include all human-readable fields,
/// repeat important ones (title, category) to increase their weight.
/// </summary>
public static class EmbeddingTextBuilder
{
    /// <summary>
    /// Build embedding text for an event.
    /// Format: "{title} {title} {categories} {hashtags} {description_excerpt}"
    /// Title is repeated to give it higher semantic weight.
    /// </summary>
    public static string ForEvent(
        string        title,
        IList<string> categories,
        IList<string> hashtags,
        string?       description = null)
    {
        var parts = new List<string>
        {
            title.Trim(),
            title.Trim(), // repeat for weight
        };

        if (categories.Count > 0)
            parts.Add(string.Join(" ", categories.Select(c => c.ToLowerInvariant())));

        if (hashtags.Count > 0)
            parts.Add(string.Join(" ", hashtags.Select(h => h.TrimStart('#').ToLowerInvariant())));

        // Trim description to first 200 chars to avoid noise overwhelming the vector
        if (!string.IsNullOrWhiteSpace(description))
            parts.Add(description.Trim()[..Math.Min(200, description.Trim().Length)]);

        return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    /// <summary>
    /// Build embedding text for a user behavior log.
    /// Format: "{actionType} {targetType} {categories} {hashtags}"
    /// Simple and focused — captures WHAT the user did and WHAT it was about.
    /// </summary>
    public static string ForBehaviorLog(
        string        actionType,
        string        targetType,
        IList<string> categories,
        IList<string> hashtags)
    {
        var parts = new List<string>
        {
            actionType.ToLowerInvariant(),
            targetType.ToLowerInvariant(),
        };

        if (categories.Count > 0)
            parts.Add(string.Join(" ", categories.Select(c => c.ToLowerInvariant())));

        if (hashtags.Count > 0)
            parts.Add(string.Join(" ", hashtags.Select(h => h.TrimStart('#').ToLowerInvariant())));

        return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}