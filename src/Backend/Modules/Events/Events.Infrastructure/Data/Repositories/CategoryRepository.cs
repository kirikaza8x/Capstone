using Events.Domain.Entities;
using Events.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;

namespace Events.Infrastructure.Data.Repositories;

public class CategoryRepository(EventsDbContext context) : RepositoryBase<Category, int>(context), ICategoryRepository
{
    private readonly EventsDbContext _context = context;

    public async Task<bool> IsCodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(c => c.Code == code, cancellationToken);
    }

    public async Task<bool> IsInUseAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.EventCategoryMappings
            .AnyAsync(x => x.CategoryId == categoryId, cancellationToken);
    }

    public async Task<List<string>> GetNamesByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        if (ids == null || !ids.Any())
            return new List<string>();

        return await _context.EventCategories
            .Where(c => ids.Contains(c.Id) && c.IsActive)
            .Select(c => c.Name)
            .ToListAsync();
    }
}
