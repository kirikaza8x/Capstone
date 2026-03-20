using Events.Domain.Enums;
using Events.Infrastructure.Data;
using Events.PublicApi.PublicApi;
using Events.PublicApi.Records;
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
                tt.SoldQuantity,
                AreaType = tt.Area != null ? tt.Area.Type : AreaType.Zone,
                tt.Event.Status,
                tt.Event.TicketSaleStartAt,
                tt.Event.TicketSaleEndAt,
                tt.Event.EventStartAt,
                ValidSessionIds = tt.Event.Sessions
                    .Where(s => sessionIds.Contains(s.Id))
                    .Select(s => s.Id)
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        // Build dictionary key = (sessionId, ticketTypeId)
        var result = new Dictionary<(Guid, Guid), EventTicketingItemDto>();

        foreach(var row in rows)
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
                    SoldQuantity: row.SoldQuantity,
                    IsPurchasable: isPurchasable);
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
}