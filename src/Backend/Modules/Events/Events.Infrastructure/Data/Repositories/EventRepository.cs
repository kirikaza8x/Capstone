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

    public async Task<Event?> GetByIdWithAllDetailsAsync(Guid id, CancellationToken cancellationToken = default)
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
}