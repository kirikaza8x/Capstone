using Shared.Domain.DDD;
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

        // Navigation properties
        public ICollection<Role> Roles { get; private set; } = new List<Role>();
        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

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
                IsActive = true,
                Roles = new List<Role>(),
                RefreshTokens = new List<RefreshToken>()
            };
            if (role != null) user.AssignRole(role);
            user.RaiseDomainEvent(new UserCreatedEvent(user.Id, user.Email ?? string.Empty, user.UserName));
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
            IsActive = false;
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

        // Refresh token management
        public RefreshToken AddRefreshToken(string token, DateTime expiry)
        {
            var refreshToken = new RefreshToken(token, expiry, Id);
            RefreshTokens.Add(refreshToken);
            return refreshToken;
        }

        public void RevokeRefreshToken(string token)
        {
            var refreshToken = RefreshTokens.FirstOrDefault(rt => rt.Token == token);
            refreshToken?.Revoke();
        }

        public void RevokeAllRefreshTokens()
        {
            foreach (var token in RefreshTokens.Where(rt => !rt.IsRevoked))
            {
                token.Revoke();
            }
        }

        public RefreshToken? GetValidRefreshToken(string token)
        {
            return RefreshTokens.FirstOrDefault(rt =>
                rt.Token == token &&
                !rt.IsRevoked &&
                rt.ExpiryDate > DateTime.UtcNow);
        }


        public RefreshToken AddRefreshToken(
        string token,
        DateTime expiry,
        string? deviceId = null,
        string? deviceName = null,
        string? ipAddress = null,
        string? userAgent = null)
        {
            var refreshToken = new RefreshToken(token, expiry, Id, deviceId, deviceName, ipAddress, userAgent);
            RefreshTokens.Add(refreshToken);
            return refreshToken;
        }

        public void RevokeRefreshTokensByDevice(string deviceId)
        {
            foreach (var token in RefreshTokens.Where(rt => rt.DeviceId == deviceId && !rt.IsRevoked))
            {
                token.Revoke();
            }
        }

        public RefreshToken? GetValidRefreshToken(string token, string? deviceId = null)
        {
            var query = RefreshTokens.Where(rt =>
                rt.Token == token &&
                !rt.IsRevoked &&
                rt.ExpiryDate > DateTime.UtcNow);

            if (!string.IsNullOrEmpty(deviceId))
            {
                query = query.Where(rt => rt.DeviceId == deviceId);
            }

            return query.FirstOrDefault();
        }

        public IEnumerable<RefreshToken> GetActiveDevices()
        {
            return RefreshTokens
                .Where(rt => !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow)
                .OrderByDescending(rt => rt.CreatedAt);
        }

        protected override void Apply(IDomainEvent @event)
        {
            switch (@event)
            {
                case UserCreatedEvent e:
                    Id = e.UserId;
                    Email = e.Email;
                    UserName = e.UserName;
                    IsActive = true;
                    break;
            }
        }
    }
}