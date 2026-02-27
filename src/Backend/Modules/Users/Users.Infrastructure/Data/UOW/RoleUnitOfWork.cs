using Roles.Domain.UOW;
using Shared.Infrastructure.Data;
using Users.Infrastructure.Persistence.Contexts;

namespace Users.Infrastructure.Data.UOW;

public class RoleUnitOfWork : UnitOfWorkBase<UserModuleDbContext>, IRoleUnitOfWork
{
    public RoleUnitOfWork(UserModuleDbContext dbContext) : base(dbContext)
    {
    }
}
