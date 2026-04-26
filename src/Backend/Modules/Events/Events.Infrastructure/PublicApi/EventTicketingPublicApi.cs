using Events.Domain.Entities;
using Events.Domain.Enums;
using Events.Infrastructure.Data;
using Events.PublicApi.PublicApi;
using Events.PublicApi.Records;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure.PublicApi;

internal sealed class EventTicketingPublicApi(EventsDbContext dbContext) : IEventTicketingPublicApi
{
    public async Task<IReadOnlyDictionary<(Guid SessionId, Guid TicketTypeId), EventTicketingItemDto>> GetTicketingItemsBatchAsync(
        IReadOnlyCollection<(Guid SessionId, Guid TicketTypeId)> pairs,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var pairSet = pairs.ToHashSet();

        var sessionIds = pairSet.Select(p => p.SessionId).ToList();
        var ticketTypeIds = pairSet.Select(p => p.TicketTypeId).ToList();

        var rows = await dbContext.TicketTypes
            .AsNoTracking()
            .Where(tt =>
                ticketTypeIds.Contains(tt.Id) &&
                tt.Event.Sessions.Any(s => sessionIds.Contains(s.Id)))
            .Select(tt => new
            {
                tt.Id,
                tt.EventId,
                tt.AreaId,
                tt.Price,
                tt.Quantity,
                AreaType = tt.Area != null ? tt.Area.Type : AreaType.Zone,
                tt.Event.Status,
                tt.Event.TicketSaleStartAt,
                tt.Event.TicketSaleEndAt,
                tt.Event.EventStartAt,
                Categories = tt.Event.EventCategories.Select(c => c.Category.Name).ToList(),
                Hashtags = tt.Event.EventHashtags.Select(h => h.Hashtag.Name).ToList(),
                ValidSessionIds = tt.Event.Sessions
                    .Where(s => sessionIds.Contains(s.Id))
                    .Select(s => s.Id)
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        // Build dictionary key = (sessionId, ticketTypeId)
        var result = new Dictionary<(Guid, Guid), EventTicketingItemDto>();

        foreach (var row in rows)
        {
            var isPurchasable =
                row.Status == EventStatus.Published &&
                row.TicketSaleStartAt.HasValue &&
                row.TicketSaleEndAt.HasValue &&
                row.TicketSaleStartAt.Value <= utcNow &&
                row.TicketSaleEndAt.Value >= utcNow &&
                (!row.EventStartAt.HasValue || row.EventStartAt.Value > utcNow);

            foreach (var sessionId in row.ValidSessionIds)
            {
                if (!pairSet.Contains((sessionId, row.Id)))
                    continue;

                result[(sessionId, row.Id)] = new EventTicketingItemDto(
                    EventId: row.EventId,
                    EventSessionId: sessionId,
                    TicketTypeId: row.Id,
                    AreaId: row.AreaId,
                    AreaType: MapAreaType(row.AreaType),
                    Price: row.Price,
                    Quantity: row.Quantity,
                    IsPurchasable: isPurchasable,
                    Categories: row.Categories,
                    Hashtags: row.Hashtags);
            }
        }

        return result;
    }

    public async Task<IReadOnlyDictionary<Guid, EventSeatDto>> GetSeatsBatchAsync(
        IReadOnlyCollection<Guid> seatIds,
        CancellationToken cancellationToken = default)
    {
        if (seatIds.Count == 0)
            return new Dictionary<Guid, EventSeatDto>();

        var seats = await dbContext.Seats
            .AsNoTracking()
            .Where(s => seatIds.Contains(s.Id))
            .Select(s => new EventSeatDto(s.Id, s.AreaId, s.SeatCode))
            .ToListAsync(cancellationToken);

        return seats.ToDictionary(s => s.SeatId);
    }

    private static EventAreaType MapAreaType(AreaType t) => t switch
    {
        AreaType.Zone => EventAreaType.Zone,
        AreaType.Seat => EventAreaType.Seat,
        _ => EventAreaType.Default
    };

    public async Task<TicketCheckInInfoDto?> GetTicketCheckInInfoAsync(
        Guid ticketTypeId,
        Guid eventSessionId,
        Guid? seatId,
        CancellationToken cancellationToken = default)
    {
        var data = await dbContext.TicketTypes
            .AsNoTracking()
            .Where(tt => tt.Id == ticketTypeId)
            .Select(tt => new
            {
                TicketTypeName = tt.Name,
                Session = tt.Event.Sessions
                    .Where(s => s.Id == eventSessionId)
                    .Select(s => new { s.Title, s.StartTime })
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (data?.Session is null)
            return null;

        string? seatCode = null;
        if (seatId.HasValue)
        {
            seatCode = await dbContext.Seats
                .AsNoTracking()
                .Where(s => s.Id == seatId.Value)
                .Select(s => s.SeatCode)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new TicketCheckInInfoDto(
            data.TicketTypeName,
            data.Session.Title,
            data.Session.StartTime,
            seatCode);
    }

    public async Task<IReadOnlyDictionary<Guid, OrderEventSummaryDto>> GetEventSummaryByEventIdsAsync(
        IReadOnlyCollection<Guid> eventIds,
        CancellationToken cancellationToken = default)
    {
        if (eventIds.Count == 0)
            return new Dictionary<Guid, OrderEventSummaryDto>();

        var rows = await dbContext.Events
            .AsNoTracking()
            .Where(e => eventIds.Contains(e.Id))
            .Select(e => new
            {
                e.Id,
                e.OrganizerId,
                e.Title,
                e.Status,
                e.BannerUrl,
                e.Location,
                e.EventStartAt
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            r => r.Id,
            r => new OrderEventSummaryDto(r.Id, r.OrganizerId, r.Title, r.Status.ToString(), r.BannerUrl, r.Location, r.EventStartAt));
    }

    public async Task<IReadOnlyDictionary<(Guid TicketTypeId, Guid EventSessionId), OrderTicketDetailDto>> GetOrderTicketDetailsAsync(
    IReadOnlyCollection<(Guid TicketTypeId, Guid EventSessionId, Guid? SeatId)> items,
    CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
            return new Dictionary<(Guid, Guid), OrderTicketDetailDto>();

        var ticketTypeIds = items.Select(i => i.TicketTypeId).Distinct().ToList();
        var sessionIds = items.Select(i => i.EventSessionId).Distinct().ToList();
        var seatIds = items
            .Where(i => i.SeatId.HasValue)
            .Select(i => i.SeatId!.Value)
            .Distinct()
            .ToList();

        var ticketTypes = await dbContext.TicketTypes
            .AsNoTracking()
            .Where(tt => ticketTypeIds.Contains(tt.Id))
            .Select(tt => new { tt.Id, tt.Name, tt.Price })
            .ToListAsync(cancellationToken);

        var sessions = await dbContext.EventSessions
            .AsNoTracking()
            .Where(s => sessionIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Title, s.StartTime })
            .ToListAsync(cancellationToken);

        var ticketTypeMap = ticketTypes.ToDictionary(tt => tt.Id, tt => (tt.Name, tt.Price));
        var sessionMap = sessions.ToDictionary(s => s.Id, s => (s.Title, s.StartTime));

        var seatCodeMap = new Dictionary<Guid, string>();
        if (seatIds.Count > 0)
        {
            var seats = await dbContext.Seats
                .AsNoTracking()
                .Where(s => seatIds.Contains(s.Id))
                .Select(s => new { s.Id, s.SeatCode })
                .ToListAsync(cancellationToken);

            seatCodeMap = seats.ToDictionary(s => s.Id, s => s.SeatCode);
        }

        // Build result — key = (TicketTypeId, EventSessionId)
        var result = new Dictionary<(Guid, Guid), OrderTicketDetailDto>();

        foreach (var item in items)
        {
            var (ticketTypeName, price) = ticketTypeMap.TryGetValue(
                item.TicketTypeId, out var tt)
                ? tt : (string.Empty, 0m);

            var (sessionTitle, sessionStartTime) = sessionMap.TryGetValue(
                item.EventSessionId, out var session)
                ? session : (string.Empty, DateTime.MinValue);

            var seatCode = item.SeatId.HasValue &&
                seatCodeMap.TryGetValue(item.SeatId.Value, out var code)
                ? code : null;

            result[(item.TicketTypeId, item.EventSessionId)] = new OrderTicketDetailDto(
                ticketTypeName,
                price,        
                sessionTitle,
                sessionStartTime,
                seatCode);
        }

        return result;
    }

    public async Task<EventDetailDto?> GetEventDetailAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var ev = await dbContext.Events
            .AsNoTracking()
            .Where(e => e.Id == eventId)
            .Select(e => new EventDetailDto(
                e.Id,
                e.OrganizerId,
                e.Title,
                e.Description,
                e.EventStartAt,
                e.Location,
                e.EventHashtags.Select(h => h.Hashtag.Name).ToList(),
                e.EventCategories.Select(c => c.Category.Name).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return ev;
    }

    public async Task<IReadOnlyList<Guid>> GetEventIdsByUserIdAsync(
        Guid organizerId,
        CancellationToken cancellationToken = default)
    {
        var eventIds = await dbContext.Events
            .AsNoTracking()
            .Where(e => e.OrganizerId == organizerId &&
                        (e.Status == EventStatus.Published || e.Status == EventStatus.Completed))
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        return eventIds;
    }

    public async Task<IReadOnlyDictionary<Guid, TicketTypeDetailDto>> GetTicketTypeDetailsAsync(
    IReadOnlyCollection<Guid> ticketTypeIds,
    CancellationToken cancellationToken = default)
    {
        if (ticketTypeIds == null || ticketTypeIds.Count == 0)
            return new Dictionary<Guid, TicketTypeDetailDto>();

        var ticketTypes = await dbContext.TicketTypes
            .Where(t => ticketTypeIds.Contains(t.Id))
            .Select(t => new TicketTypeDetailDto(
                t.Id,
                t.Name,
                t.Price,
                t.Quantity))
            .ToListAsync(cancellationToken);

        return ticketTypes.ToDictionary(t => t.Id);
    }

    public async Task<IReadOnlyCollection<TicketTypeDetailDto>> GetAllTicketTypesByEventIdAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var ticketTypes = await dbContext.TicketTypes
            .Where(t => t.EventId == eventId)
            .Select(t => new TicketTypeDetailDto(
                t.Id,
                t.Name,
                t.Price,
                t.Quantity))
            .ToListAsync(cancellationToken);

        return ticketTypes;
    }

    public async Task<IReadOnlyDictionary<Guid, OrganizerEventOverviewDto>>                     GetOrganizerEventOverviewByEventIdsAsync(
        IReadOnlyCollection<Guid> eventIds,
        CancellationToken cancellationToken = default)
    {
        if (eventIds.Count == 0)
            return new Dictionary<Guid, OrganizerEventOverviewDto>();

        var rows = await dbContext.Events
            .AsNoTracking()
            .Where(e => eventIds.Contains(e.Id))
            .Select(e => new
            {
                e.Id,
                e.Title,
                e.Status,
                e.EventStartAt,
                e.EventEndAt
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            x => x.Id,
            x => new OrganizerEventOverviewDto(
                x.Id,
                x.Title,
                x.Status.ToString(),
                x.EventStartAt,
                x.EventEndAt));
    }
}
