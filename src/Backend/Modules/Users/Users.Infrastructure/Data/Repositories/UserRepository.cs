using Users.Domain.Entities;
using Users.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Persistence.Contexts;
using Shared.Infrastructure.Data;

namespace Users.Infrastructure.Data.Repositories
{
    public class UserRepository : RepositoryBase<User, Guid>, IUserRepository
    {
        private readonly UserModuleDbContext _dbContext;
        private readonly DbSet<User> _users;

        public UserRepository(UserModuleDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _users = dbContext.Set<User>();
        }

        public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return await _users
                .Include(u => u.Roles)
                .Include(u => u.RefreshTokens)
                .AsSplitQuery()
                .AsTracking()
                .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _users
                .Include(u => u.Roles)
                .Include(u => u.RefreshTokens)
                .AsSplitQuery()
                .AsTracking()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<User?> GetByEmailOtpAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _users
                .Include(u => u.Otps)
                .AsTracking()
                .FirstOrDefaultAsync(u =>
                    u.Email == email &&
                    u.IsActive,
                    cancellationToken);
        }

        public async Task<User?> GetUserByMailOrUserNameAsync(string userNameOrEmail, CancellationToken cancellationToken = default)
        {
            return await _users
                .Include(u => u.Roles)
                .Include(u => u.RefreshTokens)
                .AsSplitQuery()
                .AsTracking()
                .FirstOrDefaultAsync(u =>
                    u.Email == userNameOrEmail || u.UserName == userNameOrEmail,
                    cancellationToken);
        }

        public async Task<User?> GetUserByMailOrUserNameAsync(IEnumerable<string> userNamesOrEmails, CancellationToken cancellationToken = default)
        {
            return await _users
                .Include(u => u.Roles)
                .Include(u => u.RefreshTokens)
                .AsSplitQuery()
                .AsTracking()
                .FirstOrDefaultAsync(u =>
                    userNamesOrEmails.Contains(u.Email ?? string.Empty)
                    || userNamesOrEmails.Contains(u.UserName ?? string.Empty),
                    cancellationToken);
        }

        public async Task<RefreshToken?> GetValidRefreshTokenForDeviceAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<RefreshToken>()
                .AsTracking()
                .FirstOrDefaultAsync(rt =>
                    rt.UserId == userId &&
                    rt.DeviceId == deviceId &&
                    !rt.IsRevoked &&
                    rt.ExpiryDate > DateTime.UtcNow,
                    cancellationToken);
        }

        public async Task<User?> LoginAsync(string userNameOrEmail, string passwordHash, CancellationToken cancellationToken = default)
        {
            return await _users
                .Include(u => u.Roles)
                .Include(u => u.RefreshTokens)
                .AsSplitQuery()
                .AsTracking()
                .FirstOrDefaultAsync(u =>
                    (u.UserName == userNameOrEmail || u.Email == userNameOrEmail)
                    && u.PasswordHash == passwordHash,
                    cancellationToken);
        }

        public Task<RefreshToken> AddOrUpdateRefreshTokenAsync(User user, RefreshToken newToken, CancellationToken cancellationToken = default)
        {
            var existingToken = user.RefreshTokens
                .FirstOrDefault(rt => rt.DeviceId == newToken.DeviceId && rt.ExpiryDate > DateTime.UtcNow && !rt.IsRevoked);

            if (existingToken != null)
            {
                existingToken.UpdateDeviceInfo(newToken.DeviceName, newToken.IpAddress, newToken.UserAgent);
                return Task.FromResult(existingToken);
            }

            user.RefreshTokens.Add(newToken);
            _dbContext.Set<RefreshToken>().Add(newToken);

            return Task.FromResult(newToken);
        }

        public async Task<User> RegisterAsync(User user, Role role, CancellationToken cancellationToken = default)
        {
            var existingRole = await _dbContext.Set<Role>()
                .FirstOrDefaultAsync(r => r.Name == role.Name, cancellationToken);

            if (existingRole != null)
            {
                user.AssignRole(existingRole);
            }
            else
            {
                user.AssignRole(role);
                _dbContext.Set<Role>().Add(role);
            }

            // Add the user
            _users.Add(user);

            // Initialize wallet for the user
            var wallet = Wallet.Create(user.Id, 1000); // Default balance of 1000, can be adjusted as needed
            user.AttachWallet(wallet);
            _dbContext.Set<Wallet>().Add(wallet);
            return user;
        }

        public async Task<User?> GetByExternalIdentityAsync(string provider, string providerKey, CancellationToken cancellationToken = default)
        {
            return await _users
                .Include(u => u.Roles)
                .Include(u => u.RefreshTokens)
                .Include(u => u.ExternalIdentities)
                .AsSplitQuery()
                .AsTracking()
                .FirstOrDefaultAsync(u =>
                    u.ExternalIdentities.Any(e => e.Provider == provider && e.ProviderKey == providerKey),
                    cancellationToken);
        }

        public async Task<User?> GetByIdWithOrganizerProfileAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Include(u => u.OrganizerProfiles)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }
    }
}
