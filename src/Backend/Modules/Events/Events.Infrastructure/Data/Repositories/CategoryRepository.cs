using Events.Domain.Entities;
using Events.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;

namespace Events.Infrastructure.Data.Repositories;

public class CategoryRepository(EventsDbContext context) : RepositoryBase<Category, int>(context), ICategoryRepository
{
    public async Task<bool> IsCodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(c => c.Code == code, cancellationToken);
    }
}
