using AI.Domain.Interfaces.UOW;
using Shared.Infrastructure.Data;

namespace AI.Infrastructure.Data.UOW;

public class AiUnitOfWork : UnitOfWorkBase<AIModuleDbContext>, IAiUnitOfWork
{
    public AiUnitOfWork(AIModuleDbContext dbContext) : base(dbContext)
    {
    }
}
