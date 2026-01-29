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
        Task<User?> GetUserByMailOrUserName(IEnumerable<string> userNamesOrEmails, CancellationToken cancellationToken = default);
        
        // ADD THIS NEW METHOD
        Task<RefreshToken?> GetValidRefreshTokenForDevice(Guid userId, string deviceId, CancellationToken cancellationToken = default);
    }
}