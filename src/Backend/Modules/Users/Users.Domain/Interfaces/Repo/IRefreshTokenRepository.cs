using Shared.Domain.Data;

namespace Users.Domain.Repositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken, Guid>
    {
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);
        Task RemoveAsync(RefreshToken token, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string token, CancellationToken cancellationToken = default);
    }
}
