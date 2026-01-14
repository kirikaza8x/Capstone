using Users.Domain.Entities;
using Shared.Domain.Data;

namespace Users.Domain.Repositories
{
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> LoginAsync(string userNameOrEmail, string password, CancellationToken cancellationToken = default);
        Task<User?> GetUserByMailOrUserName(string userNameOrEmail, CancellationToken cancellationToken = default);
        Task<User?> GetUserByMailOrUserName(
            IEnumerable<string> userNamesOrEmails,
            CancellationToken cancellationToken = default);

        // Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
        // Task<IEnumerable<RefreshToken>> GetActiveRefreshTokensByUserAsync(Guid userId, CancellationToken cancellationToken = default);
        // Task AddRefreshTokenAsync(Guid userId, RefreshToken refreshToken, CancellationToken cancellationToken = default);
        // Task RevokeRefreshTokenAsync(Guid tokenId, CancellationToken cancellationToken = default);
    }
}
