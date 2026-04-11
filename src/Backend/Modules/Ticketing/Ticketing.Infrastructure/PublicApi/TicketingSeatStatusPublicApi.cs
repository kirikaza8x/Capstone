using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Ticketing.Domain.Enums;
using Ticketing.Infrastructure.Data;
using Ticketing.PublicApi.PublicApi;

namespace Ticketing.Infrastructure.PublicApi;

internal sealed class TicketingSeatStatusPublicApi(
    TicketingDbContext dbContext,
    IConnectionMultiplexer redis) : ITicketingSeatStatusPublicApi
{
    public async Task<IReadOnlySet<Guid>> GetUnavailableSeatIdsAsync(
        Guid eventSessionId,
        IReadOnlyCollection<Guid> seatIds,
        CancellationToken cancellationToken = default)
    {
        if (seatIds.Count == 0)
            return new HashSet<Guid>();

        var seatIdList = seatIds.ToList();
        //Sold/used seats from DB
        var soldSeatIds = await dbContext.OrderTickets
            .AsNoTracking()
            .Where(ot =>
                ot.EventSessionId == eventSessionId &&
                ot.SeatId.HasValue &&
                seatIds.Contains(ot.SeatId.Value) &&
                (ot.Status == OrderTicketStatus.Valid || ot.Status == OrderTicketStatus.Used))
            .Select(ot => ot.SeatId!.Value)
            .ToListAsync(cancellationToken);

        var unavailable = soldSeatIds.ToHashSet();

        // Locked seats from Redis
        var db = redis.GetDatabase();
        var keys = seatIdList
            .Select(id => (RedisKey)$"seat_lock:{eventSessionId}:{id}")
            .ToArray();

        var values = await db.StringGetAsync(keys);
        for (var i = 0; i < keys.Length; i++)
        {
            if (values[i].HasValue)
                unavailable.Add(seatIdList[i]);
        }

        return unavailable;
    }
}
