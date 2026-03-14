using AI.Domain.Events;
using AI.Domain.ValueObjects;
using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
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

            var categories = log.GetCategories();

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

        // ── Domain helpers ────────────────────────────────────────────────────

        /// <summary>
        /// Parses categories from metadata, supporting comma, semicolon, and pipe delimiters.
        /// Checks both "categories" (plural) and "category" (singular) keys.
        /// </summary>
        public List<string> GetCategories()
        {
            if (!_metadata.TryGetValue("categories", out var value) &&
                !_metadata.TryGetValue("category", out value))
                return new List<string>();

            if (string.IsNullOrWhiteSpace(value))
                return new List<string>();

            return value
                .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim().ToLowerInvariant())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();
        }

        /// <summary>Delegates to ActionTypes — single source of truth.</summary>
        public bool IsConversion() => ActionTypes.IsConversion(ActionType);

        /// <summary>Delegates to ActionTypes — single source of truth.</summary>
        public bool IsEngagement() => ActionTypes.IsEngagement(ActionType);

        public bool IsMobile() => DeviceType is "mobile" or "tablet";

        public string? GetMetadataValue(string key) =>
            _metadata.TryGetValue(key, out var value) ? value : null;

        protected override void Apply(IDomainEvent @event)
        {
            // UserBehaviorLog is write-only and never reconstructed from events.
            // No state-mutation needed here.
        }
    }
}