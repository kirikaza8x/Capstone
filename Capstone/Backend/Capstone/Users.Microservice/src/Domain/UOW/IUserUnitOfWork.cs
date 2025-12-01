using Shared.Domain.UnitOfWork;
using Users.Domain.Repositories;

namespace Users.Domain.UnitOfWork
{
    public interface IUserUnitOfWork : IUnitOfWork
    {
        // You can add user-specific repository shortcuts here if needed
        IUserRepository Users { get; }
    }

}