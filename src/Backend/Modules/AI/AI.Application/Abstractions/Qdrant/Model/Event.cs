namespace AI.Application.Abstractions.Qdrant.Model;

/// <summary>What gets stored per event in Qdrant.</summary>
public record EventVectorPayload(
    Guid EventId,
    string Title,
    List<string> Categories,  // all categories — keyword filter + embedding signal
    List<string> Hashtags,    // all hashtags   — keyword filter + embedding signal
    DateTime EventStartAt,
    decimal? MinPrice,
    string? BannerUrl
);

/// <summary>What comes back from a similarity search.</summary>
public record EventSearchResult(
    Guid EventId,
    float Score,
    string Title,
    List<string> Categories,
    List<string> Hashtags,
    DateTime EventStartAt,
    decimal? MinPrice,
    string? BannerUrl
);