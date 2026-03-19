using Qdrant.Client.Grpc;
using QdrantRange = Qdrant.Client.Grpc.Range;

namespace Shared.Infrastructure.Qdrant.Helpers;

/// <summary>
/// Fluent builder for Qdrant Filter objects.
///
/// Eliminates the repeated pattern of:
///   var conditions = new List&lt;Condition&gt;();
///   if (x != null) conditions.Add(new Condition { Field = ... });
///   var filter = new Filter();
///   foreach (var c in conditions) filter.Must.Add(c);
///
/// Usage:
///   var filter = QdrantFilterBuilder.Must()
///       .Keyword("category", category)
///       .Keyword("user_id", userId.ToString())
///       .DateTimeAfter("occurred_at", since)
///       .Build();
///
/// Returns null if no conditions were added — compatible with SearchRawAsync(filter: null).
/// </summary>
public sealed class QdrantFilterBuilder
{
    private readonly List<Condition> _must = new();
    private readonly List<Condition> _should = new();
    private readonly List<Condition> _mustNot = new();

    private QdrantFilterBuilder() { }

    /// <summary>Start a builder where all conditions are AND'd (Must).</summary>
    public static QdrantFilterBuilder Must() => new();

    // ── Keyword match ─────────────────────────────────────────────

    /// <summary>
    /// Add an exact keyword match on a payload field.
    /// Skipped if value is null or whitespace.
    /// </summary>
    public QdrantFilterBuilder Keyword(string field, string? value, bool must = true)
    {
        if (string.IsNullOrWhiteSpace(value)) return this;

        var condition = new Condition
        {
            Field = new FieldCondition
            {
                Key = field,
                Match = new Match { Keyword = value.Trim().ToLowerInvariant() }
            }
        };

        Add(condition, must);
        return this;
    }

    /// <summary>
    /// Add an exact keyword match for a Guid field (stored as string in payload).
    /// Skipped if value is null or empty Guid.
    /// </summary>
    public QdrantFilterBuilder Keyword(string field, Guid? value, bool must = true)
    {
        if (!value.HasValue || value.Value == Guid.Empty) return this;
        return Keyword(field, value.Value.ToString(), must);
    }

    /// <summary>
    /// Add a keyword match from a list — Qdrant matches if the payload field contains ANY of the values.
    /// Skipped if list is null or empty.
    /// </summary>
    public QdrantFilterBuilder KeywordAny(string field, IReadOnlyList<string>? values, bool must = true)
    {
        if (values is null || values.Count == 0) return this;

        // One condition per value joined as Should inside a nested filter
        var nested = new Filter();
        foreach (var val in values.Where(v => !string.IsNullOrWhiteSpace(v)))
        {
            nested.Should.Add(new Condition
            {
                Field = new FieldCondition
                {
                    Key = field,
                    Match = new Match { Keyword = val.Trim().ToLowerInvariant() }
                }
            });
        }

        if (nested.Should.Count == 0) return this;

        Add(new Condition { Filter = nested }, must);
        return this;
    }

    // ── Range ─────────────────────────────────────────────────────

    /// <summary>
    /// Add a greater-than-or-equal range on a DateTime field (stored as ISO 8601 string).
    /// Qdrant datetime fields use Unix seconds internally.
    /// Skipped if value is null.
    /// </summary>
    public QdrantFilterBuilder DateTimeAfter(string field, DateTime? after, bool must = true)
    {
        if (!after.HasValue) return this;

        var condition = new Condition
        {
            Field = new FieldCondition
            {
                Key = field,
                Range = new QdrantRange { Gte = ((DateTimeOffset)after.Value).ToUnixTimeSeconds() }
            }
        };

        Add(condition, must);
        return this;
    }

    /// <summary>
    /// Add a less-than-or-equal range on a DateTime field.
    /// Skipped if value is null.
    /// </summary>
    public QdrantFilterBuilder DateTimeBefore(string field, DateTime? before, bool must = true)
    {
        if (!before.HasValue) return this;

        var condition = new Condition
        {
            Field = new FieldCondition
            {
                Key = field,
                Range = new QdrantRange { Lte = ((DateTimeOffset)before.Value).ToUnixTimeSeconds() }
            }
        };

        Add(condition, must);
        return this;
    }

    /// <summary>
    /// Add a numeric range — use for min_price, score thresholds, etc.
    /// Pass null for either bound to leave it open-ended.
    /// </summary>
    public QdrantFilterBuilder NumericRange(string field, double? gte = null, double? lte = null, bool must = true)
    {
        if (gte is null && lte is null) return this;

        var range = new QdrantRange();
        if (gte.HasValue) range.Gte = gte.Value;
        if (lte.HasValue) range.Lte = lte.Value;

        var condition = new Condition
        {
            Field = new FieldCondition { Key = field, Range = range }
        };

        Add(condition, must);
        return this;
    }

    // ── Build ─────────────────────────────────────────────────────

    /// <summary>
    /// Build the Qdrant Filter. Returns null if no conditions were added —
    /// pass directly to SearchRawAsync(filter: builder.Build()).
    /// </summary>
    public Filter? Build()
    {
        if (_must.Count == 0 && _should.Count == 0 && _mustNot.Count == 0)
            return null;

        var filter = new Filter();
        foreach (var c in _must) filter.Must.Add(c);
        foreach (var c in _should) filter.Should.Add(c);
        foreach (var c in _mustNot) filter.MustNot.Add(c);
        return filter;
    }

    // ── Private ───────────────────────────────────────────────────

    private void Add(Condition condition, bool must)
    {
        if (must) _must.Add(condition);
        else _should.Add(condition);
    }
}