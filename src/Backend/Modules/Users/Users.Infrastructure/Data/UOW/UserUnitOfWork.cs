using Shared.Infrastructure.Data;
using Users.Domain.UOW;
using Users.Infrastructure.Persistence.Contexts;

namespace Users.Infrastructure.Data.UOW;

public class UserUnitOfWork : UnitOfWorkBase<UserModuleDbContext>, IUserUnitOfWork
{
    public UserUnitOfWork(UserModuleDbContext dbContext) : base(dbContext)
    {
    }
}
