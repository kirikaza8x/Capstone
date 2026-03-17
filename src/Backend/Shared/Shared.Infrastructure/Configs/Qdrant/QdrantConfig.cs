namespace Shared.Infrastructure.Configs.Qdrant;

/// <summary>
/// Qdrant connection + collection config.
/// Collections are keyed by logical name (e.g. "Events", "UserBehavior")
/// so adding a new collection requires zero code changes — just a new config block.
/// </summary>
public class QdrantConfig : ConfigBase
{
    public override string SectionName => "Qdrant";

    // ── Connection ────────────────────────────────────────────────
    public string Host     { get; set; } = "localhost";
    public int    Port     { get; set; } = 6334;
    public bool   UseHttps { get; set; } = false;
    public string ApiKey   { get; set; } = "";

    /// <summary>
    /// ── Collections ───────────────────────────────────────────────
    /// Key = logical name used by repos (e.g. "Events", "UserBehavior")
    /// Value = physical Qdrant collection name + vector size
    /// </summary>
    public Dictionary<string, QdrantCollectionConfig> Collections { get; set; } = new();

    /// <summary>
    /// Typed accessor — throws clearly if a required collection key is missing.
    /// Use in concrete repo constructors: config.Get("Events")
    /// </summary>
    public QdrantCollectionConfig Get(string key)
        => Collections.TryGetValue(key, out var cfg)
            ? cfg
            : throw new InvalidOperationException(
                $"Qdrant collection config missing for key '{key}'. " +
                $"Add it under Qdrant:Collections:{key} in appsettings.");
}

/// <summary>
/// Per-collection settings — physical name + vector dimensionality.
/// Keep these together so adding a collection is one config block, not two scattered keys.
/// </summary>
public class QdrantCollectionConfig
{
    public string Name       { get; set; } = "";
    public int    VectorSize { get; set; } = 384;
}