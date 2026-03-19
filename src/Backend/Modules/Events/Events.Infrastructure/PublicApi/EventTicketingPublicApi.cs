using Events.Domain.Entities;
using Events.Domain.Enums;
using Events.Infrastructure.Data;
using Events.PublicApi.PublicApi;
using Events.PublicApi.Records;
using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure.PublicApi;

internal sealed class EventTicketingPublicApi(EventsDbContext dbContext) : IEventTicketingPublicApi
{
    public async Task<EventTicketingItemDto?> GetTicketingItemAsync(
        Guid eventSessionId,
        Guid ticketTypeId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var ticketType = await dbContext.TicketTypes
            .AsNoTracking()
            .Include(x => x.Event)
            .Include(x => x.Area)
            .FirstOrDefaultAsync(x => x.Id == ticketTypeId, cancellationToken);

        if (ticketType is null)
            return null;

        var sessionBelongsToEvent = await dbContext.EventSessions
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == eventSessionId && x.EventId == ticketType.EventId,
                cancellationToken);

        if (!sessionBelongsToEvent)
            return null;

        return new EventTicketingItemDto
        {
            EventId = ticketType.EventId,
            EventSessionId = eventSessionId,
            TicketTypeId = ticketType.Id,
            AreaId = ticketType.AreaId,
            AreaType = MapAreaType(ticketType.Area?.Type),
            Price = ticketType.Price,
            Quantity = ticketType.Quantity,
            SoldQuantity = ticketType.SoldQuantity,
            IsPurchasable = IsPurchasable(ticketType.Event, utcNow)
        };
    }

    public async Task<EventSeatDto?> GetSeatAsync(
        Guid seatId,
        CancellationToken cancellationToken = default)
    {
        var seat = await dbContext.Seats
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == seatId, cancellationToken);

        if (seat is null)
            return null;

        return new EventSeatDto
        {
            SeatId = seat.Id,
            AreaId = seat.AreaId
        };
    }

    private static EventAreaType MapAreaType(AreaType? areaType)
    {
        return areaType switch
        {
            AreaType.Seat => EventAreaType.Seat,
            AreaType.Default => EventAreaType.Default,
            _ => EventAreaType.Zone
        };
    }

    private static bool IsPurchasable(Event @event, DateTime utcNow)
    {
        if (@event.Status != EventStatus.Published)
            return false;

        if (!@event.TicketSaleStartAt.HasValue || !@event.TicketSaleEndAt.HasValue)
            return false;

        if (@event.TicketSaleStartAt.Value > utcNow || @event.TicketSaleEndAt.Value < utcNow)
            return false;

        if (@event.EventStartAt.HasValue && @event.EventStartAt.Value <= utcNow)
            return false;

        return true;
    }
}