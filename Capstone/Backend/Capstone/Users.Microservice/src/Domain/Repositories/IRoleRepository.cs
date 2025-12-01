using Users.Domain.Entities;
using Shared.Domain.Repositories;

namespace Users.Domain.Repositories
{
    public interface IRoleRepository : IRepository<Role>
    {
        // Task<Role?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
        // Task<Role?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        // Task<Role?> LoginAsync(string userNameOrEmail, string password, CancellationToken cancellationToken = default);
        // Task<Role?> GetUserByMailOrUserName(string userNameOrEmail, CancellationToken cancellationToken = default);

    }
}

