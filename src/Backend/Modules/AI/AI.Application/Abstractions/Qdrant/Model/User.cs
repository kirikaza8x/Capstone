
namespace AI.Application.Abstractions.Qdrant.Model;
/// <summary>
/// What gets stored per behavior log in Qdrant.
/// Mirrors UserBehaviorLog domain entity — no domain types leak into infra.
/// </summary>
public record UserBehaviorVectorPayload(
    Guid         LogId,
    Guid         UserId,
    string       ActionType,   // "view", "search", "bookmark", "purchase"
    string       TargetId,     // EventId or search query id
    string       TargetType,   // "event", "search_query"
    List<string> Categories,   // extracted from metadata
    List<string> Hashtags,     // extracted from metadata
    string?      SessionId,
    string?      DeviceType,
    DateTime     OccurredAt
);

/// <summary>Result of finding behavior logs similar to a query vector.</summary>
public record BehaviorSearchResult(
    Guid         LogId,
    Guid         UserId,
    float        Score,
    string       ActionType,
    string       TargetId,
    List<string> Categories,
    List<string> Hashtags,
    DateTime     OccurredAt
);