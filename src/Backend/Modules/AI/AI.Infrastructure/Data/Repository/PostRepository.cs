using AI.Infrastructure.Data;
using Marketing.Domain.Entities;
using Marketing.Domain.Enums;
using Marketing.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;

namespace Marketing.Infrastructure.Persistence.Repositories;

public class PostRepository
    : RepositoryBase<PostMarketing, Guid>,
      IPostRepository
{
    public PostRepository(AIModuleDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<PostMarketing>> GetByEventAndOrganizerAsync(
        Guid eventId, Guid organizerId, CancellationToken ct = default)
    {
        return await Query()
            .AsNoTracking()
            .Where(p => p.EventId == eventId && p.OrganizerId == organizerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PostMarketing>> GetByOrganizerAsync(
        Guid organizerId, PostStatus? status = null, CancellationToken ct = default)
    {
        var query = Query().AsNoTracking().Where(p => p.OrganizerId == organizerId);
        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);
        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PostMarketing>> GetPendingQueueAsync(CancellationToken ct = default)
    {
        return await Query()
            .AsNoTracking()
            .Where(p => p.Status == PostStatus.Pending)
            .OrderBy(p => p.SubmittedAt ?? p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PostMarketing>> GetPublishedByEventAsync(
        Guid eventId, CancellationToken ct = default)
    {
        return await Query()
            .AsNoTracking()
            .Where(p => p.EventId == eventId && p.Status == PostStatus.Published)
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PostMarketing>> GetGlobalFeedAsync(
        int limit, CancellationToken ct = default)
    {
        return await Query()
            .AsNoTracking()
            .Where(p => p.Status == PostStatus.Published)
            .OrderByDescending(p => p.PublishedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<PostMarketing?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;
        return await Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug.Trim().ToLowerInvariant(), ct);
    }

    public async Task<PostMarketing?> GetByTrackingTokenAsync(string trackingToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(trackingToken)) return null;
        return await Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.TrackingToken == trackingToken.Trim().ToLowerInvariant(), ct);
    }

    public async Task<bool> TrackingTokenExistsAsync(string trackingToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(trackingToken)) return false;
        return await Query()
            .AsNoTracking()
            .AnyAsync(p => p.TrackingToken == trackingToken.Trim().ToLowerInvariant(), ct);
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return false;
        return await Query()
            .AsNoTracking()
            .AnyAsync(p => p.Slug == slug.Trim().ToLowerInvariant(), ct);
    }

    // public async Task<IReadOnlyList<PostMarketing>> GetDispatchedPostsForPlatformAsync(
    //     string platform, CancellationToken ct = default)
    // {
    //     var normalised = platform.Trim().ToLowerInvariant();

    //     // Include via the public Distributions property (not the field name string)
    //     // so EF can resolve the navigation correctly.
    //     // No AsNoTracking — the cron only reads, but EF needs to fix up the
    //     // collection into the private field for GetConfirmedDistributions() to work.
    //     return await Query()
    //         .Include(p => p.Distributions)
    //         .Where(p =>
    //             p.Status == PostStatus.Published &&
    //             p.Distributions.Any(d =>
    //                 d.Platform == normalised &&
    //                 d.ExternalPostId != null))
    //         .ToListAsync(ct);
    // }

    public async Task<IReadOnlyList<PostMarketing>> GetPublishedNotDistributedToAsync(
        ExternalPlatform platform,
        int limit = 50,
        CancellationToken ct = default)
    {
        return await Query()
            .AsNoTracking()
            .Where(p => p.Status == PostStatus.Published)
            .Where(p => !p.ExternalDistributions.Any(d =>
                d.Platform == platform && d.Status == DistributionStatus.Sent))
            .OrderByDescending(p => p.PublishedAt)
            .Take(limit)
            .ToListAsync(ct);
    }
    public async Task<ExternalDistribution?> GetDistributionByPostAndPlatformAsync(
        Guid postId,
        ExternalPlatform platform,
        CancellationToken ct = default)
    {
        return await Context.Set<ExternalDistribution>()
            .AsNoTracking()
            .FirstOrDefaultAsync(d =>
                d.PostMarketingId == postId &&
                d.Platform == platform,
                ct);
    }

    public async Task<PostMarketing?> GetByIdWithDistributionsAsync(
        Guid id,
        CancellationToken ct = default)
    {
        return await Query()
            .Include(p => p.ExternalDistributions)  // ← Load child entities
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }
}