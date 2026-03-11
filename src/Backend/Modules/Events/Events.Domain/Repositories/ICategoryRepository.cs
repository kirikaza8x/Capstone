using Events.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace Events.Domain.Repositories;

public interface ICategoryRepository : IRepository<Category, int>
{
    Task<bool> IsCodeExistsAsync(string code, CancellationToken cancellationToken = default);
}