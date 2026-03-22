using Events.Domain.Entities;
using Events.Domain.Enums;
using Events.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.Extensions;

namespace Events.Infrastructure.Data.Repositories;

internal sealed class EventRepository(EventsDbContext context)
    : RepositoryBase<Event, Guid>(context), IEventRepository
{
    private readonly EventsDbContext _context = context;

    public async Task<Event?> GetByIdWithSessionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Sessions)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Event?> GetByIdWithTicketTypesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.TicketTypes)
                .ThenInclude(t => t.Area)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Event?> GetByIdWithAreasAndSeatsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Areas)
                .ThenInclude(a => a.Seats)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Event?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Images)
            .Include(e => e.ActorImages)
            .Include(e => e.Sessions)
            .Include(e => e.TicketTypes)
                .ThenInclude(t => t.Area)
            .Include(e => e.EventHashtags)
                .ThenInclude(eh => eh.Hashtag)
            .Include(e => e.EventCategories)
                .ThenInclude(ec => ec.Category)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetByOrganizerIdAsync(Guid organizerId, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Where(e => e.OrganizerId == organizerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetByStatusAsync(EventStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Where(e => e.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<Event?> GetByUrlPathAsync(string urlPath, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .FirstOrDefaultAsync(e => e.UrlPath == urlPath, cancellationToken);
    }

    public async Task<bool> IsUrlPathExistsAsync(string urlPath, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .AnyAsync(e => e.UrlPath == urlPath, cancellationToken);
    }

    public async Task<EventSession?> GetEventSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.EventSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
    }

    public async Task<Event?> GetByIdWithImagesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Images)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Event>> GetPublishedWithCategoriesAsync(
        PagedQuery pagedQuery,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Events
            .AsNoTracking()
            .Where(e => e.Status == EventStatus.Published)
            .Include(e => e.EventCategories)
                .ThenInclude(ec => ec.Category)
            .AsSplitQuery();

        return await query.ToPagedResultAsync(pagedQuery, cancellationToken);
    }

    public async Task<PagedResult<Event>> GetByOrganizerPagedAsync(
        Guid organizerId,
        EventStatus? status,
        PagedQuery pagedQuery,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Events
            .AsNoTracking()
            .Where(e => e.OrganizerId == organizerId)
            .Where(e => status == null || e.Status == status)
            .Include(e => e.EventCategories)
                .ThenInclude(ec => ec.Category)
            .AsSplitQuery();

        if (string.IsNullOrWhiteSpace(pagedQuery.SortColumn))
        {
            query = query
                .OrderByDescending(e => e.Status == EventStatus.Published)
                .ThenByDescending(e => e.CreatedAt);
        }

        return await query.ToPagedResultAsync(pagedQuery, cancellationToken);
    }

    public async Task<Event?> GetByIdWithTicketTypesAndAreasAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.TicketTypes)
            .Include(e => e.Areas)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Event?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Members)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<bool> HasPermissionAsync(Guid eventId, Guid userId, string permission, CancellationToken cancellationToken = default)
    {
        var isOwner = await _context.Events
            .AnyAsync(e => e.Id == eventId && e.OrganizerId == userId, cancellationToken);

        if (isOwner)
            return true;

        return await _context.EventMembers
            .AnyAsync(m =>
                m.EventId == eventId &&
                m.UserId == userId &&
                m.Permissions.Contains(permission),
                cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetByCategoriesOrHashtagsAsync(
    IEnumerable<string> categoryNames,
    IEnumerable<string> hashtagNames,
    CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Where(e =>
                e.EventCategories.Any(ec => categoryNames.Contains(ec.Category.Name)) ||
                e.EventHashtags.Any(eh => hashtagNames.Contains(eh.Hashtag.Name)))
            .Include(e => e.EventCategories)
                .ThenInclude(ec => ec.Category)
            .Include(e => e.EventHashtags)
                .ThenInclude(eh => eh.Hashtag)
            .Include(e => e.TicketTypes)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetPublishedEndedEventsAsync(
        DateTime utcNow,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Where(e =>
                e.Status == EventStatus.Published &&
                e.EventEndAt.HasValue &&
                e.EventEndAt <= utcNow)
            .OrderBy(e => e.EventEndAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetEventsDueReminderAsync(
        DateTime utcNow,
        DateTime toUtc,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Where(e =>
                e.Status == EventStatus.Published &&
                e.IsEmailReminderEnabled &&
                e.EventStartAt.HasValue &&
                e.EventStartAt > utcNow &&
                e.EventStartAt <= toUtc &&
                e.ReminderTriggeredAt == null
             )
            .OrderBy(e => e.EventStartAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetSuspendedExpiredEventsAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Where(e =>
                e.Status == EventStatus.Suspended &&
                e.SuspendedUntilAt.HasValue &&
                e.SuspendedUntilAt > fromUtc &&
                e.SuspendedUntilAt <= toUtc)
            .OrderBy(e => e.SuspendedUntilAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetAllActivePagedAsync(
    int page, int pageSize, CancellationToken ct = default)
    {
        return await _context.Set<Event>()
            .AsNoTracking()
            .Where(e => e.IsActive)
            .Include(e => e.EventCategories).ThenInclude(ec => ec.Category)
            .Include(e => e.EventHashtags).ThenInclude(eh => eh.Hashtag)
            .Include(e => e.TicketTypes)
            .OrderBy(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<Event?> GetByIdForReIndexAsync(
    Guid id, CancellationToken ct = default)
    {
        return await _context.Events
            .AsNoTracking()
            .Where(e => e.Id == id && e.IsActive)
            .Include(e => e.EventCategories).ThenInclude(ec => ec.Category)
            .Include(e => e.EventHashtags).ThenInclude(eh => eh.Hashtag)
            .Include(e => e.TicketTypes)
            .AsSplitQuery()
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<TicketType>> GetTicketTypesByIdsAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken = default) =>
        await _context.TicketTypes
            .Where(tt => ids.Contains(tt.Id))
            .ToListAsync(cancellationToken);
}
