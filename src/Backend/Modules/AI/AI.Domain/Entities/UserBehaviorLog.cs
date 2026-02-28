using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    /// <summary>
    /// Immutable audit log of user interactions.
    /// WRITE-ONLY: Never updated, only created.
    /// PURPOSE: Forms the foundation for all scoring, trend analysis, and ML features.
    /// </summary>
    public class UserBehaviorLog : AggregateRoot<Guid>
    {
        // ===== IDENTITY & CONTEXT =====
        public Guid UserId { get; private set; }

        // ===== THE "WHAT" =====
        public string ActionType { get; private set; } = default!;   // e.g., "click", "view", "purchase"
        public string TargetId { get; private set; } = default!;     // e.g., "product-123", "article-456"
        public string TargetType { get; private set; } = default!;   // e.g., "item", "category", "content"

        // ===== THE "WHEN" (Critical for Time Decay) =====
        public DateTime OccurredAt { get; private set; }

        // ===== FLEXIBLE METADATA =====
        // Backing field for EF Core JSON serialization
        private Dictionary<string, string> _metadata = new();
        public IReadOnlyDictionary<string, string> Metadata => _metadata.AsReadOnly();

        // ===== OPTIMIZATION: Session tracking for contextual features =====
        // public string? SessionId { get; private set; }
        // public string? DeviceType { get; private set; }  // mobile, desktop, tablet

        private UserBehaviorLog() { }

        public static UserBehaviorLog Create(
            Guid userId,
            string actionType,
            string targetId,
            string targetType,
            Dictionary<string, string>? metadata = null,
            string? sessionId = null,
            string? deviceType = null)
        {
            // ===== GUARD CLAUSES: Fail fast =====
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));

            if (string.IsNullOrWhiteSpace(actionType))
                throw new ArgumentException("ActionType is required.", nameof(actionType));

            if (string.IsNullOrWhiteSpace(targetId))
                throw new ArgumentException("TargetId is required.", nameof(targetId));

            if (string.IsNullOrWhiteSpace(targetType))
                throw new ArgumentException("TargetType is required.", nameof(targetType));

            return new UserBehaviorLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ActionType = actionType.Trim().ToLowerInvariant(),
                TargetId = targetId.Trim(),
                TargetType = targetType.Trim().ToLowerInvariant(),
                OccurredAt = DateTime.UtcNow,
                _metadata = metadata ?? new Dictionary<string, string>(),
                // SessionId = sessionId,
                // DeviceType = deviceType?.ToLowerInvariant()
            };
        }

        // ===== DOMAIN HELPERS =====

        /// <summary>
        /// Robust category parser supporting multiple formats:
        /// - Single: "music"
        /// - Comma-separated: "music, jazz"
        /// - Semicolon-separated: "music; jazz"
        /// - Mixed delimiters: "music, jazz; blues"
        /// </summary>
        public List<string> GetCategories()
        {
            // Try both "categories" (plural) and "category" (singular) keys
            if (!_metadata.TryGetValue("categories", out var value) &&
                !_metadata.TryGetValue("category", out value))
            {
                return new List<string>();
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return new List<string>();
            }

            // Normalize delimiters and clean data
            return value
                .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim().ToLowerInvariant())
                .Where(c => !string.IsNullOrWhiteSpace(c))  // Extra safety
                .Distinct()  // Remove duplicates like "Jazz, jazz"
                .ToList();
        }

        /// <summary>
        /// Checks if this action is a high-value conversion event.
        /// NOTE: This logic should ideally live in InteractionWeight config,
        /// but this helper is useful for quick filtering in analytics.
        /// </summary>
        public bool IsConversion()
        {
            return ActionType is "purchase" or "subscribe" or "checkout" or "signup";
        }

        /// <summary>
        /// Checks if this is an engagement action (not just a passive view)
        /// </summary>
        public bool IsEngagement()
        {
            return ActionType is "click" or "like" or "share" or "comment" or "bookmark";
        }

        /// <summary>
        /// Gets a metadata value safely, returning null if not found
        /// </summary>
        public string? GetMetadataValue(string key)
        {
            return _metadata.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Checks if this log is from a mobile device
        /// </summary>
        // public bool IsMobile()
        // {
        //     return DeviceType is "mobile" or "tablet";
        // }

        protected override void Apply(IDomainEvent @event)
        {
            // Implement event sourcing logic if needed
            // Example: UserBehaviorLogCreated event
        }
    }
}