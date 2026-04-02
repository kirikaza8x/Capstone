using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence.Contexts;

namespace Users.Infrastructure.Data.Repositories
{
    public class PolicyRepository : RepositoryBase<Policy, Guid>, IPolicyRepository
    {
        private readonly UserModuleDbContext _dbContext;

        public PolicyRepository(UserModuleDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Policy>> GetListAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Policy>()
                .AsNoTracking()
                .OrderBy(x => x.Type)
                .ToListAsync(cancellationToken);
        }
    }
}
