using Events.Domain.Entities;
using Events.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;

namespace Events.Infrastructure.Data.Repositories;

public class HashtagRepository(EventsDbContext context)
    : RepositoryBase<Hashtag, int>(context), IHashtagRepository
{
    private readonly EventsDbContext _context = context;

    public async Task<bool> IsSlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(h => h.Slug == slug, cancellationToken);
    }

    public async Task<bool> IsInUseAsync(int hashtagId, CancellationToken cancellationToken = default)
    {
        return await _context.EventHashtags
            .AnyAsync(x => x.HashtagId == hashtagId, cancellationToken);
    }

    public async Task<List<string>> GetNamesByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        if (ids == null || !ids.Any())
            return new List<string>();

        return await _context.Hashtags
            .Where(h => ids.Contains(h.Id) && h.IsActive)
            .Select(h => h.Name)
            .ToListAsync();
    }
}