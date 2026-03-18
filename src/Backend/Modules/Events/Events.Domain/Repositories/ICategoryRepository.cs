using Events.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace Events.Domain.Repositories;

public interface ICategoryRepository : IRepository<Category, int>
{
    Task<bool> IsCodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> IsInUseAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<List<string>> GetNamesByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
}