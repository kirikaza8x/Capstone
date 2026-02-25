using Shared.Domain.DDD;
using Users.Domain.Events;
using Users.Domain.Enums;

namespace Users.Domain.Entities
{
    public partial class User : AggregateRoot<Guid>
    {
        // --------------------
        // Identity
        // --------------------
        public string? Email { get; private set; }
        public string UserName { get; private set; } = default!;
        public string PasswordHash { get; private set; } = default!;

        // --------------------
        // Status
        // --------------------
        public UserStatus Status { get; private set; }

        // EF Core constructor
        private User() { }

        // --------------------
        // Factory
        // --------------------
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
                Address = address
            };

            if (role != null)
                user.AssignRole(role);

            user.Status = UserStatus.Active;
            user.RaiseDomainEvent(
                new UserCreatedEvent(user.Id, user.Email ?? string.Empty, user.UserName)
            );

            return user;
        }

        public void ChangeEmail(string newEmail) => Email = newEmail;
        public void ChangePassword(string newPasswordHash) => PasswordHash = newPasswordHash;

        public void Activate() => Status = UserStatus.Active;
        public void Deactivate() => Status = UserStatus.Inactive;
        public void Ban() => Status = UserStatus.Banned;
    }
}