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

    // ─────────────────────────────────────────────────────────────
    // Organizer Queries
    // ─────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<PostMarketing>> GetByEventAndOrganizerAsync(
        Guid eventId,
        Guid organizerId,
        CancellationToken ct = default)
    {
        return await Query()
            .AsNoTracking()
            .Where(p => p.EventId == eventId && p.OrganizerId == organizerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PostMarketing>> GetByOrganizerAsync(
        Guid organizerId,
        PostStatus? status = null,
        CancellationToken ct = default)
    {
        var query = Query()
            .AsNoTracking()
            .Where(p => p.OrganizerId == organizerId);

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    // ─────────────────────────────────────────────────────────────
    // Admin / Moderation
    // ─────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<PostMarketing>> GetPendingQueueAsync(
        CancellationToken ct = default)
    {
        return await Query()
            .AsNoTracking()
            .Where(p => p.Status == PostStatus.Pending)
            .OrderBy(p => p.SubmittedAt ?? p.CreatedAt)
            .ToListAsync(ct);
    }

    // ─────────────────────────────────────────────────────────────
    // Public Queries
    // ─────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<PostMarketing>> GetPublishedByEventAsync(
        Guid eventId,
        CancellationToken ct = default)
    {
        return await Query()
            .AsNoTracking()
            .Where(p => p.EventId == eventId && p.Status == PostStatus.Published)
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PostMarketing>> GetGlobalFeedAsync(
        int limit,
        CancellationToken ct = default)
    {
        return await Query()
            .AsNoTracking()
            .Where(p => p.Status == PostStatus.Published)
            .OrderByDescending(p => p.PublishedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<PostMarketing?> GetBySlugAsync(
        string slug,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return null;

        slug = slug.Trim().ToLowerInvariant();

        return await Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug, ct);
    }

    // ─────────────────────────────────────────────────────────────
    // Tracking
    // ─────────────────────────────────────────────────────────────

    public async Task<PostMarketing?> GetByTrackingTokenAsync(
        string trackingToken,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(trackingToken))
            return null;

        trackingToken = trackingToken.Trim().ToLowerInvariant();

        return await Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.TrackingToken == trackingToken, ct);
    }

    public async Task<bool> TrackingTokenExistsAsync(
        string trackingToken,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(trackingToken))
            return false;

        trackingToken = trackingToken.Trim().ToLowerInvariant();

        return await Query()
            .AsNoTracking()
            .AnyAsync(p => p.TrackingToken == trackingToken, ct);
    }

    public async Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return false;

        slug = slug.Trim().ToLowerInvariant();

        return await Query()
            .AsNoTracking()
            .AnyAsync(p => p.Slug == slug, ct);
    }
}