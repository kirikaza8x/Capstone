using Shared.Domain.DDD;
using Users.Domain.Events;
using Users.Domain.Enums;
using Users.Domain.Errors.Users;

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



        public void ChangeEmail(string newEmail) => Email = newEmail;
        public void ChangePassword(string newHash)
        {
            if (this.PasswordHash == newHash)
            {
                throw new InvalidOperationException(UserErrors.SamePassword.Code);
            }

            this.PasswordHash = newHash;
        }



        public void Activate() => Status = UserStatus.Active;
        public void Deactivate() => Status = UserStatus.Inactive;
        public void Ban() => Status = UserStatus.Banned;
    }
}