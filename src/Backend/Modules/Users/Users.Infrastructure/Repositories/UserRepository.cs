using Users.Domain.Entities;
using Users.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Persistence.Contexts;
using Shared.Infrastructure.Repository;

namespace Users.Infrastructure.Repositories
{
    public class UserRepository : RepositoryBase<User, Guid>, IUserRepository
    {
        private readonly UserModuleDbContext _context;
        private readonly DbSet<User> _dbSet;

        public UserRepository(UserModuleDbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<User>();
        }

        public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<User?> LoginAsync(string userNameOrEmail, string password, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => (u.UserName == userNameOrEmail || u.Email == userNameOrEmail) && u.PasswordHash == password, cancellationToken);
        }

        public async Task<User?> GetUserByMailOrUserName(string userNameOrEmail, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.Roles)
                .Where(u => u.Email == userNameOrEmail || u.UserName == userNameOrEmail)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<User?> GetUserByMailOrUserName(
            IEnumerable<string> userNamesOrEmails,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.Roles)
                .Where(u => userNamesOrEmails.Contains(u.Email ?? string.Empty)
                         || userNamesOrEmails.Contains(u.UserName ?? string.Empty))
                .FirstOrDefaultAsync(cancellationToken);
        }
        public async Task<RefreshToken?> GetValidRefreshTokenForDevice(
            Guid userId, 
            string deviceId, 
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<RefreshToken>()
                .Where(rt => 
                    rt.UserId == userId && 
                    rt.DeviceId == deviceId &&
                    !rt.IsRevoked && 
                    rt.ExpiryDate > DateTime.UtcNow)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}