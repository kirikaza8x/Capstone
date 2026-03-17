using Qdrant.Client.Grpc;

namespace Shared.Infrastructure.Qdrant.Helpers;

/// <summary>
/// Safe payload field readers for Qdrant ScoredPoint / RetrievedPoint payloads.
///
/// Eliminates the repeated pattern of:
///   p.TryGetValue("field", out var v) ? v.StringValue : fallback
///   p.TryGetValue("field", out var v) ? v.ListValue.Values.Select(...) : new List&lt;string&gt;()
///
/// All methods are null-safe and return sensible defaults.
///
/// Usage:
///   var reader = new QdrantPayloadReader(hit.Payload);
///   Guid   id         = reader.GetGuid("event_id");
///   string title      = reader.GetString("title");
///   var    hashtags   = reader.GetStringList("hashtags");
///   var    occurredAt = reader.GetDateTime("occurred_at");
///   decimal? price    = reader.GetDecimal("min_price");
/// </summary>
public sealed class QdrantPayloadReader
{
    private readonly IDictionary<string, Value> _payload;

    public QdrantPayloadReader(IDictionary<string, Value> payload)
    {
        _payload = payload ?? throw new ArgumentNullException(nameof(payload));
    }

    // ── Primitives ────────────────────────────────────────────────

    /// <summary>Returns the string value or empty string if missing/blank.</summary>
    public string GetString(string field, string fallback = "")
    {
        if (_payload.TryGetValue(field, out var v) && !string.IsNullOrWhiteSpace(v.StringValue))
            return v.StringValue;
        return fallback;
    }

    /// <summary>Returns the string value or null if missing/blank.</summary>
    public string? GetStringOrNull(string field)
    {
        if (_payload.TryGetValue(field, out var v) && !string.IsNullOrWhiteSpace(v.StringValue))
            return v.StringValue;
        return null;
    }

    /// <summary>Parses a Guid from a string field. Throws if field is required and missing/invalid.</summary>
    public Guid GetGuid(string field)
    {
        var raw = GetString(field);
        if (!Guid.TryParse(raw, out var guid))
            throw new InvalidOperationException(
                $"Qdrant payload field '{field}' is missing or not a valid Guid. Value: '{raw}'");
        return guid;
    }

    /// <summary>Parses a DateTime from an ISO 8601 string field. Returns default if missing.</summary>
    public DateTime GetDateTime(string field)
    {
        var raw = GetString(field);
        return DateTime.TryParse(raw, out var dt) ? dt : default;
    }

    /// <summary>Parses a decimal from a string field. Returns null if missing or not parseable.</summary>
    public decimal? GetDecimal(string field)
    {
        var raw = GetStringOrNull(field);
        return decimal.TryParse(raw, out var d) ? d : null;
    }

    /// <summary>Parses a double from a DoubleValue field. Returns null if missing.</summary>
    public double? GetDouble(string field)
    {
        if (_payload.TryGetValue(field, out var v))
            return v.DoubleValue;
        return null;
    }

    /// <summary>Parses a bool from a BoolValue field. Returns false if missing.</summary>
    public bool GetBool(string field)
    {
        if (_payload.TryGetValue(field, out var v))
            return v.BoolValue;
        return false;
    }

    // ── Lists ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns a list of strings from a Qdrant ListValue field.
    /// Returns empty list if field is missing or has no entries.
    /// </summary>
    public List<string> GetStringList(string field)
    {
        if (_payload.TryGetValue(field, out var v) && v.ListValue?.Values?.Count > 0)
            return v.ListValue.Values
                .Select(x => x.StringValue)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

        return new List<string>();
    }

    /// <summary>
    /// Returns a list of Guids from a Qdrant ListValue field (each entry stored as string).
    /// Skips entries that fail to parse.
    /// </summary>
    public List<Guid> GetGuidList(string field)
    {
        if (_payload.TryGetValue(field, out var v) && v.ListValue?.Values?.Count > 0)
            return v.ListValue.Values
                .Select(x => x.StringValue)
                .Where(s => Guid.TryParse(s, out _))
                .Select(Guid.Parse)
                .ToList();

        return new List<Guid>();
    }
}