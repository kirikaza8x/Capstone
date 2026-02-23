namespace Users.Domain.Entities
{
    public partial class User
    {
        // --------------------
        // Auth
        // --------------------
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
        }
    }
}