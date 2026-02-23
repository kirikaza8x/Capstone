using Shared.Domain.DDD;

namespace Users.Domain.Entities
{
    public class ExternalIdentity : Entity<Guid>
    {
        public Guid UserId { get; private set; }
        public string Provider { get; private set; } = default!;   // e.g. "Google", "Facebook"
        public string ProviderKey { get; private set; } = default!; // unique ID from provider
        public DateTime LinkedAt { get; private set; }

        // EF Core constructor
        private ExternalIdentity() { }

        public static ExternalIdentity Create(Guid userId, string provider, string providerKey)
        {
            var identity = new ExternalIdentity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Provider = provider,
                ProviderKey = providerKey,
                CreatedAt = DateTime.UtcNow,
                LinkedAt = DateTime.UtcNow
            };

            return identity;
        }

        // Domain behaviors
        public void UpdateProviderKey(string newProviderKey)
        {
            ProviderKey = newProviderKey;
        }
    }
}
