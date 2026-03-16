namespace AI.Domain.Helpers;
public class MetadataHelper
{
    private readonly IReadOnlyDictionary<string, string> _metadata;

    public MetadataHelper(IReadOnlyDictionary<string, string> metadata)
    {
        _metadata = metadata;
    }

    /// <summary>
    /// Retrieves a single metadata value by key(s).
    /// Returns empty string if not found or blank.
    /// </summary>
    public string GetValue(params string[] keys)
    {
        foreach (var key in keys)
        {
            if (_metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }
        return string.Empty;
    }

    /// <summary>
    /// Retrieves a list of metadata values by key(s), splitting on delimiters.
    /// Returns empty list if not found or blank.
    /// </summary>
    public List<string> GetList(string[] keys, char[] delimiters = null!)
    {
        delimiters ??= new[] { ',', ';', '|', '#' };

        foreach (var key in keys)
        {
            if (_metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value
                    .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => v.Trim().ToLowerInvariant())
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Distinct()
                    .ToList();
            }
        }
        return new List<string>();
    }

}
