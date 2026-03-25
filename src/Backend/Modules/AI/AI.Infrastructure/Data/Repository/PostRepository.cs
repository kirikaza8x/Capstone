// File: Marketing.Infrastructure/Persistence/Repositories/PostRepository.cs
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
            .Where(p => p.EventId == eventId && p.OrganizerId == organizerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PostMarketing>> GetByOrganizerAsync(
        Guid organizerId,
        PostStatus? status = null,
        CancellationToken ct = default)
    {
        var query = Query().Where(p => p.OrganizerId == organizerId);

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    // ─────────────────────────────────────────────────────────────
    // Admin / Moderation Queries
    // ─────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<PostMarketing>> GetPendingQueueAsync(
        CancellationToken ct = default)
    {
        return await Query()
            .Where(p => p.Status == PostStatus.Pending)
            .OrderBy(p => p.SubmittedAt) // FIFO: oldest submissions first
            .ToListAsync(ct);
    }

    // ─────────────────────────────────────────────────────────────
    // Public / Attendee Queries
    // ─────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<PostMarketing>> GetPublishedByEventAsync(
        Guid eventId,
        CancellationToken ct = default)
    {
        return await Query()
            .Where(p => p.EventId == eventId && p.Status == PostStatus.Published)
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync(ct);
    }

    // ─────────────────────────────────────────────────────────────
    // Attribution / Tracking
    // ─────────────────────────────────────────────────────────────

    public async Task<PostMarketing?> GetByTrackingTokenAsync(
        string trackingToken,
        CancellationToken ct = default)
    {
        return await Query()
            .FirstOrDefaultAsync(p => p.TrackingToken == trackingToken, ct);
    }

    public async Task<bool> TrackingTokenExistsAsync(
        string trackingToken,
        CancellationToken ct = default)
    {
        return await Query()
            .AnyAsync(p => p.TrackingToken == trackingToken, ct);
    }
}