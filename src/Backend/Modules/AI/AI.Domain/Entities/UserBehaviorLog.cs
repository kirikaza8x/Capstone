using Shared.Domain.DDD; 

namespace AI.Domain.Entities
{
    public class UserBehaviorLog : AggregateRoot<Guid>
    {
        // 1. Identity & Context
        public Guid UserId { get; private set; }

        // 2. The "What"
        public string ActionType { get; private set; } = default!; // e.g., "click", "view"
        public string TargetId { get; private set; } = default!;   // e.g., "product-123"
        public string TargetType { get; private set; } = default!; // e.g., "item", "category"

        // 3. The "When" (Crucial for Time Decay!)
        // Renamed from 'lastOccurredAt'. A log never changes, so it's just 'OccurredAt'.
        public DateTime OccurredAt { get; private set; }

        // 4. Flexible Data
        // specific backing field for EF Core / ORM JSON serialization support
        private Dictionary<string, string> _metadata = new();
        public IReadOnlyDictionary<string, string> Metadata => _metadata.AsReadOnly();

        private UserBehaviorLog() { }

        public static UserBehaviorLog Create(
            Guid userId,
            string actionType,
            string targetId,
            string targetType,
            Dictionary<string, string>? metadata = null)
        {
            // GUARD CLAUSES: Fail fast if data is invalid
            if (string.IsNullOrWhiteSpace(actionType)) throw new ArgumentException("ActionType is required.");
            if (string.IsNullOrWhiteSpace(targetId)) throw new ArgumentException("TargetId is required.");

            return new UserBehaviorLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ActionType = actionType.Trim().ToLowerInvariant(),
                TargetId = targetId.Trim(),
                TargetType = targetType.Trim().ToLowerInvariant(),
                OccurredAt = DateTime.UtcNow,
                _metadata = metadata ?? new Dictionary<string, string>()
            };
        }

        // ----------------------------
        // Domain Helpers (Optimized)
        // ----------------------------

        /// <summary>
        /// Robust category parser.
        /// Handles: "Music", "Music, Jazz", " Music ; Jazz "
        /// </summary>
        public List<string> GetCategories()
        {
            if (!_metadata.TryGetValue("categories", out var value) && 
                !_metadata.TryGetValue("category", out value))
            {
                return new List<string>();
            }

            // Normalize delimiters (handle comma or semicolon)
            return value
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim().ToLowerInvariant())
                .Distinct() // Avoid duplicates like "Jazz, jazz"
                .ToList();
        }

        /// <summary>
        /// Checks if this action represents a Conversion.
        /// NOTE: It is often better to keep this logic in the "InteractionWeight" config,
        /// but keeping a helper here is fine for quick filtering.
        /// </summary>
        public bool IsConversion()
        {
            return ActionType is "purchase" or "subscribe" or "checkout";
        }

        protected override void Apply(IDomainEvent @event) { }
    }
}