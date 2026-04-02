using Microsoft.EntityFrameworkCore;
using Shared.Application.Abstractions.Time;
using StackExchange.Redis;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;
using Ticketing.Infrastructure.Data;
using Ticketing.PublicApi;
using Ticketing.PublicApi.Records;

namespace Ticketing.Infrastructure.PublicApi;

internal sealed class TicketingPublicApi(
    TicketingDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    IConnectionMultiplexer redis) : ITicketingPublicApi
{
    public async Task<OrderDetails?> GetOrderAsync(
    Guid orderId,
    Guid userId,
    bool requirePaid = false,
    CancellationToken cancellationToken = default)
    {
        var query = dbContext.Orders
            .Include(o => o.Tickets)
            .AsNoTracking()
            .Where(o => o.Id == orderId && o.UserId == userId);

        if (requirePaid)
        {
            query = query.Where(o => o.Status == OrderStatus.Paid);
        }

        var order = await query.FirstOrDefaultAsync(cancellationToken);

        if (order is null) return null;

        var validTickets = order.Tickets
            .Where(t => t.Status != Ticketing.Domain.Enums.OrderTicketStatus.Cancelled)
            .ToList();

        if (validTickets.Count == 0) return null;

        var pricePerTicket = order.TotalPrice / validTickets.Count;

        return new OrderDetails(
            OrderId: order.Id,
            UserId: order.UserId,
            EventId: order.EventId,
            TotalAmount: order.TotalPrice,
            Tickets: validTickets
                .Select(t => new OrderTicketDetail(
                    OrderTicketId: t.Id,
                    EventSessionId: t.EventSessionId,
                    Amount: pricePerTicket))
                .ToList());
    }
    public async Task<VoucherValidationResult?> ValidateOrderVoucherAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        // Load the voucher attached to this order (if any)
        var orderVoucher = await dbContext.OrderVouchers
            .Include(ov => ov.Voucher)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                ov => ov.OrderId == orderId,
                cancellationToken);

        // No voucher attached — nothing to validate, proceed with payment
        if (orderVoucher is null)
            return null;

        // Re-validate MaxUse at payment initiation time (authoritative check)
        if (orderVoucher.Voucher.TotalUse >= orderVoucher.Voucher.MaxUse)
            return new VoucherValidationResult(
                IsValid: false,
                ErrorMessage: $"Voucher '{orderVoucher.Voucher.CouponCode}' has reached its maximum usage limit.");

        return new VoucherValidationResult(IsValid: true);
    }


    public async Task<IReadOnlyDictionary<Guid, int>> GetZoneLockedCountsAsync(
        Guid eventSessionId,
        IReadOnlyCollection<Guid> ticketTypeIds,
        CancellationToken cancellationToken = default)
    {
        if (ticketTypeIds.Count == 0)
            return new Dictionary<Guid, int>();

        var db = redis.GetDatabase();
        var idList = ticketTypeIds.ToList();

        var keys = idList
            .Select(id => (RedisKey)$"zone_lock:{eventSessionId}:{id}")
            .ToArray();

        var values = await db.StringGetAsync(keys);

        var result = new Dictionary<Guid, int>();
        for (var i = 0; i < idList.Count; i++)
        {
            result[idList[i]] = values[i].HasValue
                ? (int)values[i]
                : 0;
        }

        return result;
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetSeatLockedCountsByTicketTypeAsync(
        Guid eventSessionId,
        IReadOnlyCollection<Guid> ticketTypeIds,
        CancellationToken cancellationToken = default)
    {
        if (ticketTypeIds.Count == 0)
            return new Dictionary<Guid, int>();

        var lockedCounts = await dbContext.OrderTickets
            .AsNoTracking()
            .Where(ot =>
                ot.EventSessionId == eventSessionId &&
                ticketTypeIds.Contains(ot.TicketTypeId) &&
                ot.SeatId.HasValue &&
                ot.Order.Status == OrderStatus.Pending)
            .GroupBy(ot => ot.TicketTypeId)
            .Select(g => new { TicketTypeId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return lockedCounts.ToDictionary(x => x.TicketTypeId, x => x.Count);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetSoldCountsAsync(Guid eventSessionId, IEnumerable<Guid> ticketTypeIds, CancellationToken cancellationToken)
    {
        var ticketTypeIdList = ticketTypeIds.ToList();

        if (ticketTypeIdList.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        // Query 
        var counts = await dbContext.Set<OrderTicket>()
            .Where(t => t.EventSessionId == eventSessionId
                     && ticketTypeIdList.Contains(t.TicketTypeId)
                     && (t.Status == OrderTicketStatus.Valid || t.Status == OrderTicketStatus.Used))
            .GroupBy(t => t.TicketTypeId)
            .Select(g => new
            {
                TicketTypeId = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var result = new Dictionary<Guid, int>();

        foreach (var item in counts)
        {
            result[item.TicketTypeId] = item.Count;
        }

        return result;
    }

    public async Task<IReadOnlyCollection<Guid>> GetOrdersByEventIdAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var orderIds = await dbContext.Orders
            .AsNoTracking()
            .Where(ot => ot.EventId == eventId && ot.Status == OrderStatus.Paid)
            .Select(ot => ot.Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        return orderIds;
    }

    public async Task<TicketingMetricsDto> GetTicketingMetricsAsync(CancellationToken cancellationToken = default)
    {
        var now = dateTimeProvider.UtcNow;
        // get metrics for current 30-day period and previous 30-day period
        var startOfCurrentPeriod = now.AddDays(-30);
        var startOfLastPeriod = now.AddDays(-60);

        var totalRevenueTask = dbContext.Orders
            .Where(o => o.Status == OrderStatus.Paid)
            .SumAsync(o => o.TotalPrice, cancellationToken);

        var totalTicketsSoldTask = dbContext.OrderTickets
            .CountAsync(t => t.Status == OrderTicketStatus.Valid || t.Status == OrderTicketStatus.Used, cancellationToken);

        // Get the revenue from the last 30 days.
        var currentPeriodRevenueTask = dbContext.Orders
            .Where(o => o.Status == OrderStatus.Paid && o.CreatedAt >= startOfCurrentPeriod)
            .SumAsync(o => o.TotalPrice, cancellationToken);

        // Revenue from the previous 30-day period
        var lastPeriodRevenueTask = dbContext.Orders
            .Where(o => o.Status == OrderStatus.Paid && o.CreatedAt >= startOfLastPeriod && o.CreatedAt < startOfCurrentPeriod)
            .SumAsync(o => o.TotalPrice, cancellationToken);

        await Task.WhenAll(
            totalRevenueTask,
            totalTicketsSoldTask,
            currentPeriodRevenueTask,
            lastPeriodRevenueTask);

        decimal currentRev = currentPeriodRevenueTask.Result;
        decimal lastRev = lastPeriodRevenueTask.Result;
        double growthRate = 0;

        if (lastRev == 0)
        {
            growthRate = currentRev > 0 ? 100.0 : 0.0;
        }
        else
        {
            growthRate = Math.Round((double)((currentRev - lastRev) / lastRev * 100), 1);
        }

        return new TicketingMetricsDto(
            TotalRevenue: totalRevenueTask.Result,
            RevenueGrowthRate: growthRate,
            TotalTicketsSold: totalTicketsSoldTask.Result);
    }

    public async Task<IReadOnlyList<DailySalesTrendDto>> GetSalesTrendAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
    {
        // Get data and group by date
        var rawData = await dbContext.Orders
            .Where(o => o.Status == OrderStatus.Paid
                     && o.CreatedAt != null 
                     && o.CreatedAt >= startDate
                     && o.CreatedAt <= endDate)
            .GroupBy(o => o.CreatedAt.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.TotalPrice),
                Transactions = g.Count()
            })
            .ToListAsync(cancellationToken);

        var dict = rawData.ToDictionary(x => x.Date);
        var result = new List<DailySalesTrendDto>();

        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            if (dict.TryGetValue(date, out var data))
            {
                result.Add(new DailySalesTrendDto(date, data.Revenue, data.Transactions));
            }
            else
            {
                result.Add(new DailySalesTrendDto(date, 0, 0));
            }
        }

        return result;
    }

    public async Task<IReadOnlyList<TopEventTicketMetricsDto>> GetTopEventsMetricsAsync(
            int top,
            DateTime? startDate = null,
            CancellationToken cancellationToken = default)
    {
        var query = dbContext.Orders
            .Where(o => o.Status == OrderStatus.Paid);

        if (startDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= startDate.Value);
        }

        // get top events by revenue
        var topRevenueEvents = await query
            .GroupBy(o => o.EventId)
            .Select(g => new
            {
                EventId = g.Key,
                TotalRevenue = g.Sum(o => o.TotalPrice)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(top)
            .ToListAsync(cancellationToken);

        if (topRevenueEvents.Count == 0)
            return new List<TopEventTicketMetricsDto>();

        // get list of event ids in the top list to filter tickets in the next query
        var topEventIds = topRevenueEvents.Select(e => e.EventId).ToList();

        // BƯỚC 2: Đếm số vé bán ra NHƯNG CHỈ đếm cho các sự kiện nằm trong Top ở trên.
        var ticketCounts = await dbContext.OrderTickets
            .Where(t => topEventIds.Contains(t.Order.EventId)
                     && t.Order.Status == OrderStatus.Paid
                     && (t.Status == OrderTicketStatus.Valid || t.Status == OrderTicketStatus.Used))
            .Where(t => !startDate.HasValue || t.Order.CreatedAt >= startDate.Value)
            .GroupBy(t => t.Order.EventId)
            .Select(g => new
            {
                EventId = g.Key,
                TicketsSold = g.Count()
            })
            .ToDictionaryAsync(x => x.EventId, x => x.TicketsSold, cancellationToken);

        var result = topRevenueEvents.Select(e => new TopEventTicketMetricsDto(
            e.EventId,
            e.TotalRevenue,
            ticketCounts.TryGetValue(e.EventId, out var count) ? count : 0 
        )).ToList();

        return result;
    }
}
