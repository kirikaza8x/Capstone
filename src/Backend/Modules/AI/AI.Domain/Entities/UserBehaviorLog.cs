using Shared.Domain.DDD;

namespace AI.Domain.Entities
{
    public class UserBehaviorLog : AggregateRoot<Guid>
    {
        public Guid SessionId { get; private set; }
        public Guid UserId { get; private set; }
        public Guid EventId { get; private set; }
        public string ActionType { get; private set; } = default!;
        public string? Metadata { get; private set; }
        
        // Learning fields - track if action led to conversion
        public bool LedToConversion { get; private set; }
        public Guid? ConversionEventId { get; private set; }
        public DateTime? ConversionTimestamp { get; private set; }

        private UserBehaviorLog() { }

        public static UserBehaviorLog Create(
            Guid sessionId,
            Guid userId,
            Guid eventId,
            string actionType,
            string? metadata = null)
        {
            return new UserBehaviorLog
            {
                Id = Guid.NewGuid(), 
                SessionId = sessionId,
                UserId = userId,
                EventId = eventId,
                ActionType = actionType,
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow,
                LedToConversion = false
            };
        }

        // Mark when user converts after this action
        public void MarkAsLeadingToConversion(Guid conversionEventId)
        {
            LedToConversion = true;
            ConversionEventId = conversionEventId;
            ConversionTimestamp = DateTime.UtcNow;
        }

        public void AttachMetadata(string key, string value)
        {
            Metadata = string.IsNullOrEmpty(Metadata)
                ? $"{key}:{value}"
                : $"{Metadata};{key}:{value}";
        }

        // Helper to identify high-value actions
        public bool IsHighValueAction()
        {
            return ActionType.ToLower() switch
            {
                "purchase" => true,
                "register" => true,
                "ticket_purchase" => true,
                "share" => true,
                _ => false
            };
        }

        /// <summary>
        /// Extract category from metadata
        /// Metadata format: "category:music;other:value"
        /// </summary>
        public string? GetCategory()
        {
            if (string.IsNullOrEmpty(Metadata))
                return null;

            var parts = Metadata.Split(';');
            var categoryPart = parts.FirstOrDefault(p => p.StartsWith("category:", StringComparison.OrdinalIgnoreCase));
            
            if (categoryPart == null)
                return null;

            return categoryPart.Split(':')[1].ToLowerInvariant();
        }
        /// <summary>
        /// Get all categories from metadata
        /// Metadata format: "categories:music,jazz,concert;other:value"
        /// </summary>
        public List<string> GetCategories()
        {
            if (string.IsNullOrEmpty(Metadata))
                return new List<string>();

            var parts = Metadata.Split(';');
            var categoriesPart = parts.FirstOrDefault(p => p.StartsWith("categories:", StringComparison.OrdinalIgnoreCase));
            
            if (categoriesPart == null)
            {
                // Try singular "category"
                var singleCategory = GetCategory();
                return singleCategory != null ? new List<string> { singleCategory } : new List<string>();
            }

            var categoriesValue = categoriesPart.Split(':')[1];
            return categoriesValue.Split(',')
                .Select(c => c.Trim().ToLowerInvariant())
                .Where(c => !string.IsNullOrEmpty(c))
                .ToList();
        }

        protected override void Apply(IDomainEvent @event)
        {
            // Event sourcing hook
        }
    }
}