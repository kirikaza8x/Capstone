using Users.Domain.Entities;
using Users.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Persistence.Contexts;
using Shared.Infrastructure.Data;

namespace Users.Infrastructure.Data.Repositories
{
    public class RoleRepository : RepositoryBase<Role,Guid>, IRoleRepository
    {
        private readonly UserModuleDbContext _context;
        private readonly DbSet<Role> _dbSet;

        public RoleRepository(UserModuleDbContext context) : base(context)
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
