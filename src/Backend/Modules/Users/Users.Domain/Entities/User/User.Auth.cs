using Users.Domain.Events;

namespace Users.Domain.Entities
{
    public partial class User
    {
        // --------------------
        // Auth
        // --------------------
        public bool IsVerified { get; private set; } // General name for all providers

        public ICollection<Role> Roles { get; private set; } = new List<Role>();
        public ICollection<ExternalIdentity> ExternalIdentities { get; private set; } = new List<ExternalIdentity>();

        // --------------------
        // Roles
        // --------------------
        public void AssignRole(Role role)
        {
            if (Roles.Any(r => r.Id == role.Id || r.Name == role.Name))
                return;

            Roles.Add(role);
        }

        public void RemoveRole(Role role)
        {
            if (Roles.Contains(role))
                Roles.Remove(role);
        }

        // --------------------
        // External Identities
        // --------------------
        public void BindExternalIdentity(string provider, string providerKey)
        {
            if (ExternalIdentities.Any(e =>
                e.Provider == provider && e.ProviderKey == providerKey))
                return;

            ExternalIdentities.Add(
                ExternalIdentity.Create(Id, provider, providerKey)
            );
            Verify();
        }

        public void UnbindExternalIdentity(string provider, string providerKey)
        {
            var identity = ExternalIdentities.FirstOrDefault(e =>
                e.Provider == provider && e.ProviderKey == providerKey);

            if (identity != null)
                ExternalIdentities.Remove(identity);
        }

        public static User CreateExternal(
            string email,
            string userName,
            string provider,
            string providerKey,
            string? firstName = null,
            string? lastName = null)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = userName,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = string.Empty,
                IsVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            user.BindExternalIdentity(provider, providerKey);

            user.RaiseDomainEvent(new UserCreatedEvent(user.Id, email, userName));

            return user;
        }

        public void Verify() => IsVerified = true;

    }
}