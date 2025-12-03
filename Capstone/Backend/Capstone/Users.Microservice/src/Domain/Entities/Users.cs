
using Shared.Domain.Common.DDD;
using Users.Domain.Events;
namespace Users.Domain.Entities
{
    public class User : AggregateRoot<Guid>
    {
        // Identity
        public string? Email { get; private set; } = default!;
        public string UserName { get; private set; } = default!;
        public string? FirstName { get; private set; }
        public string? LastName { get; private set; }
        public string PasswordHash { get; private set; } = default!;
        public string? PhoneNumber { get; private set; }
        public string? Address { get; private set; }
        public string? ProfileImageUrl { get; private set; }
        public string? RefreshToken { get; private set; }
        public DateTime? RefreshTokenExpiry { get; private set; }
        public ICollection<Role> Roles { get; private set; } = new List<Role>();


        // EF Core constructor
        private User() { }

        public static User Create(
            string? email,
            string userName,
            string passwordHash,
            string? firstName = null,
            string? lastName = null,
            string? phoneNumber = null,
            string? address = null,
            string? profileImageUrl = null,
            Role? role = null)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = userName,
                PasswordHash = passwordHash,
                ProfileImageUrl = profileImageUrl,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                Address = address,
                IsDeleted = false,
                Roles = new List<Role>(),
            };
            if (role != null) user.AssignRole(role);
                user.RaiseEvent(new UserCreatedEvent(user.Id, user.Email ?? string.Empty, user.UserName));
            return user;
        }


        // Domain behaviors
        public void ChangeEmail(string newEmail)
        {

            Email = newEmail;
            // RaiseEvent(new UserEmailChangedEvent(Id, newEmail));
        }

        public void ChangePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            // RaiseEvent(new UserPasswordChangedEvent(Id));
        }

        public void UpdateProfile(string? firstName, string? lastName, string? phone, string? address, string? profileImageUrl)
        {
            FirstName = firstName ?? FirstName;
            LastName = lastName ?? LastName;
            PhoneNumber = phone ?? PhoneNumber;
            Address = address ?? Address;
            ProfileImageUrl = profileImageUrl ?? ProfileImageUrl;
        }


        public void DeactivateUser()
        {
            IsDeleted = false;
            // RaiseEvent(new UserDeactivatedEvent(Id));
        }

        public void AssignRole(Role role)
        {
            // Check by Id (preferred if Role is an entity)
            if (Roles.Any(r => r.Id == role.Id))
                return;

            // Or check by Name if that's the unique identifier
            if (Roles.Any(r => r.Name == role.Name))
                return;

            Roles.Add(role);
        }


        public void RemoveRole(Role role)
        {
            if (Roles.Contains(role))
                Roles.Remove(role);
        }


        public void SetRefreshToken(string token, DateTime expiry)
        {
            RefreshToken = token;
            RefreshTokenExpiry = expiry;
        }


        protected override void Apply(IDomainEvent @event)
        {
            // No-op since events are turned off
        }
    }
}
