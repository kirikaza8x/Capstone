using Shared.Domain.Data.Repositories;
using Users.Domain.Entities;

namespace Users.Domain.Repositories
{
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetUserByMailOrUserNameAsync(string userNameOrEmail, CancellationToken cancellationToken = default);
        Task<User?> GetUserByMailOrUserNameAsync(IEnumerable<string> userNamesOrEmails, CancellationToken cancellationToken = default);
        Task<RefreshToken?> GetValidRefreshTokenForDeviceAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default);
        Task<User?> LoginAsync(string userNameOrEmail, string passwordHash, CancellationToken cancellationToken = default);
        Task<RefreshToken> AddOrUpdateRefreshTokenAsync(User user, RefreshToken newToken, CancellationToken cancellationToken = default);
        Task<User> RegisterAsync(User user, Role role, CancellationToken cancellationToken = default);
        Task<User?> GetByExternalIdentityAsync(string provider, string providerKey, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailOtpAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetByIdWithOrganizerProfileAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<User?> GetByIdWithRoleAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<User?> GetByIdWithTokenAsync(Guid userId, CancellationToken cancellationToken = default);

    }
}
