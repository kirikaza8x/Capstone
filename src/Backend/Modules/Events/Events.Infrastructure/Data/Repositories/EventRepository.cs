using Events.Domain.Entities;
using Events.Domain.Enums;
using Events.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;

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

    public async Task<Event?> GetByIdWithAreasAndSeatsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Include(e => e.Areas)
                .ThenInclude(a => a.Seats)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Event?> GetByIdWithAllDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
        .Include(e => e.Images)
        .Include(e => e.Sessions)
        .Include(e => e.EventHashtags)
            .ThenInclude(eh => eh.Hashtag)
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

    public Task<(IReadOnlyList<Event> Events, int TotalCount)> GetPagedAsync(string? searchTerm = null, EventStatus? status = null, int? categoryId = null, Guid? organizerId = null, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 10, string? sortBy = null, bool isDescending = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}