using Users.Domain.Entities;
using Shared.Domain.Repositories;

namespace Users.Domain.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> LoginAsync(string userNameOrEmail, string password, CancellationToken cancellationToken = default);
        Task<User?> GetUserByMailOrUserName(string userNameOrEmail, CancellationToken cancellationToken = default);

    }
}

