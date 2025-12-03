using Users.Domain.Entities;
using Shared.Domain.Repositories;

namespace Users.Domain.Repositories
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role?> GetByRoleNameAsync(string roleName);

    }
}

