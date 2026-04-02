using Shared.Domain.Data.Repositories;
using Users.Domain.Entities;

namespace Users.Domain.Repositories
{
    public interface IPolicyRepository : IRepository<Policy, Guid>
    {
        Task<IReadOnlyList<Policy>> GetListAsync(CancellationToken cancellationToken = default);
    }
}
