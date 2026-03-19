using AI.Domain.Events;
using AI.Domain.Helpers;
using AI.Domain.ValueObjects;
using Shared.Domain.DDD;

namespace AI.Domain.Entities;

/// <summary>
/// Immutable audit log of a single user interaction.
/// WRITE-ONLY: never updated after creation.
/// Forms the foundation for scoring, trend analysis, and ML features.
/// </summary>
public class UserBehaviorLog : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string ActionType { get; private set; } = default!;
    public string TargetId { get; private set; } = default!;
    public string TargetType { get; private set; } = default!;
    public string? SessionId { get; private set; }
    public string? DeviceType { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private Dictionary<string, string> _metadata = new();
    public IReadOnlyDictionary<string, string> Metadata => _metadata.AsReadOnly();

    private UserBehaviorLog() { }

    public static UserBehaviorLog Create(
        Guid userId,
        string actionType,
        string targetId,
        string targetType,
        IReadOnlyDictionary<string, string>? metadata = null,
        string? sessionId = null,
        string? deviceType = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        if (string.IsNullOrWhiteSpace(actionType))
            throw new ArgumentException("ActionType is required.", nameof(actionType));
        if (string.IsNullOrWhiteSpace(targetId))
            throw new ArgumentException("TargetId is required.", nameof(targetId));
        if (string.IsNullOrWhiteSpace(targetType))
            throw new ArgumentException("TargetType is required.", nameof(targetType));

        var normalizedAction = actionType.Trim().ToLowerInvariant();

        var log = new UserBehaviorLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ActionType = normalizedAction,
            TargetId = targetId.Trim(),
            TargetType = targetType.Trim().ToLowerInvariant(),
            SessionId = sessionId,
            DeviceType = deviceType?.Trim().ToLowerInvariant(),
            OccurredAt = DateTime.UtcNow,
            _metadata = metadata is not null
                ? metadata.ToDictionary(
                    k => k.Key.Trim().ToLowerInvariant(),
                    v => v.Value.Trim())
                : new Dictionary<string, string>()
        };

        log.RaiseDomainEvent(new BehaviorLogCreatedEvent(
            LogId: log.Id,
            UserId: log.UserId,
            ActionType: log.ActionType,
            TargetId: log.TargetId,
            TargetType: log.TargetType,
            OccurredAt: log.OccurredAt,
            Metadata: log.Metadata
        ));

        return log;
    }

    // ── Metadata helpers ──────────────────────────────────────────

    /// <summary>
    /// Parses categories from metadata.
    /// Checks "categories" (plural) then "category" (singular).
    /// </summary>
    public List<string> GetCategories()
        => new MetadataHelper(_metadata)
            .GetList(new[] { "categories", "category" });

    /// <summary>
    /// Parses hashtags from metadata.
    /// Checks "hashtags" (plural) then "hashtag" (singular).
    /// Strips leading # before normalising.
    /// </summary>
    public List<string> GetHashtags()
        => new MetadataHelper(_metadata)
            .GetList(new[] { "hashtags", "hashtag" })
            .Select(h => h.TrimStart('#'))
            .Where(h => !string.IsNullOrWhiteSpace(h))
            .Distinct()
            .ToList();

    public string? GetMetadataValue(string key) =>
        _metadata.TryGetValue(key, out var value) ? value : null;

    // ── Action type helpers ───────────────────────────────────────

    /// <summary>True when action is a conversion (purchase, subscribe, checkout, signup).</summary>
    public bool IsConversion() => ActionTypes.IsConversion(ActionType);

    /// <summary>True when action is engagement (click, like, share, comment, bookmark).</summary>
    public bool IsEngagement() => ActionTypes.IsEngagement(ActionType);

    public bool IsMobile() => DeviceType is "mobile" or "tablet";

    protected override void Apply(IDomainEvent @event)
    {
        // Write-only — never reconstructed from events.
    }
}
