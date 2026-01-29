using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Repository;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence.Contexts;

namespace Users.Infrastructure.Repositories
{
    public class RefreshTokenRepository 
        : RepositoryBase<RefreshToken, Guid>, IRefreshTokenRepository
    {
        private readonly UserModuleDbContext _context;
        private readonly DbSet<RefreshToken> _dbSet;

        public RefreshTokenRepository(UserModuleDbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<RefreshToken>();
        }

        public async Task<RefreshToken?> GetByTokenAsync(
            string token, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        }

        public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(
            Guid userId, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(rt => rt.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            RefreshToken token, 
            CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(token, cancellationToken);
        }

        public Task RemoveAsync(
            RefreshToken token, 
            CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(token);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(
            string token, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(rt => rt.Token == token, cancellationToken);
        }
    }
}
