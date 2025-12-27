using Users.Domain.Entities;
using Users.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Persistence.Contexts;
using Shared.Infrastructure.Common;

namespace Users.Infrastructure.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        private readonly UserDbContext _context;
        private readonly DbSet<Role> _dbSet;

        public RoleRepository(UserDbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<Role>();
        }

        public Task<Role?> GetByRoleNameAsync(string roleName, CancellationToken cancellationToken = default)
        {
            return _dbSet.FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower(), cancellationToken);
        }
    }
}
