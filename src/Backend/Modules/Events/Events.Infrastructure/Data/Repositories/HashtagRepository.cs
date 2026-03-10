using Events.Domain.Entities;
using Events.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;

namespace Events.Infrastructure.Data.Repositories;

public class HashtagRepository(EventsDbContext context)
    : RepositoryBase<Hashtag, int>(context), IHashtagRepository
{
    public async Task<bool> IsSlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(h => h.Slug == slug, cancellationToken);
    }
}