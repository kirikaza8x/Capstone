using Shared.Domain.Data.Repositories;
using Users.Domain.Entities;

namespace Users.Domain.Repositories
{
    public interface IRoleRepository : IRepository<Role, Guid>
    {
        Task<Role?> GetByRoleNameAsync(string roleName, CancellationToken cancellationToken = default);
    }
}
