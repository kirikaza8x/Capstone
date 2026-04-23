using Events.PublicApi.PublicApi;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Time;
using Shared.Domain.Abstractions;
using Ticketing.PublicApi;

namespace Reports.Application.Admin.Queries.GetEventRevenueDetails;

internal sealed class GetEventRevenueDetailsQueryHandler(
    IEventPublicApi eventPublicApi,
    IEventTicketingPublicApi eventTicketingPublicApi,
    ITicketingPublicApi ticketingPublicApi,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetEventRevenueDetailsQuery, EventRevenueDetailsResponse>
{
    public async Task<Result<EventRevenueDetailsResponse>> Handle(
        GetEventRevenueDetailsQuery query,
        CancellationToken cancellationToken)
    {
        var eventMap = await eventPublicApi.GetEventMapByIdsAsync([query.EventId], cancellationToken);
        if (!eventMap.ContainsKey(query.EventId))
        {
            return Result.Failure<EventRevenueDetailsResponse>(
                Error.NotFound("Event.NotFound", $"Event '{query.EventId}' was not found."));
        }

        var now = dateTimeProvider.UtcNow;
        var (periodStartUtc, periodEndUtc) = GetCurrentPeriodRange(query.Period, now);

        var eventMetrics = await ticketingPublicApi.GetTopEventsMetricsAsync(
            top: 1,
            startDate: null,
            allowedEventIds: [query.EventId],
            cancellationToken);

        var eventMetric = eventMetrics.FirstOrDefault();

        var totalRevenueBeforeRefund = eventMetric?.TotalRevenue ?? 0m;
        var totalTicketsSold = eventMetric?.TicketsSold ?? 0;
        var totalRefundAmount = 0m;
        var refundOrderCount = 0;
        var netRevenue = totalRevenueBeforeRefund - totalRefundAmount;

        var ticketTypes = await eventTicketingPublicApi.GetAllTicketTypesByEventIdAsync(
            query.EventId,
            cancellationToken);

        var totalIssuedTickets = ticketTypes.Sum(x => x.Quantity);

        var occupancyRate = totalIssuedTickets > 0
            ? Math.Round((double)totalTicketsSold / totalIssuedTickets * 100d, 2)
            : 0d;

        var averageRevenuePerTicket = totalTicketsSold > 0
            ? Math.Round(totalRevenueBeforeRefund / totalTicketsSold, 2)
            : 0m;

        var byTicketType = ticketTypes
            .Select(x => new TicketTypeRevenueDto(
                TicketTypeName: x.Name,
                ListedPrice: x.Price,
                DiscountedPrice: x.Price,
                IssuedQuantity: x.Quantity,
                SoldQuantity: 0,
                CancelledOrRefundedQuantity: 0,
                Revenue: 0m,
                ContributionRate: 0d,
                Status: "selling"))
            .ToList();

        var byTime = new List<RevenueByTimeDto>
        {
            new(
                TimeLabel: BuildPeriodLabel(query.Period, periodStartUtc, periodEndUtc),
                TicketsSoldInPeriod: totalTicketsSold,
                RevenueInPeriod: totalRevenueBeforeRefund)
        };

        var profit = new ProfitSummaryDto(
            GrossProfit: netRevenue,
            ProfitMargin: netRevenue > 0m ? 100d : 0d);

        var refunds = new RefundSummaryDto(
            RefundOrderCount: refundOrderCount,
            TotalRefundAmount: totalRefundAmount,
            RefundRate: totalRevenueBeforeRefund > 0m
                ? Math.Round((double)(totalRefundAmount / totalRevenueBeforeRefund * 100m), 2)
                : 0d);

        var response = new EventRevenueDetailsResponse(
            Overview: new RevenueOverviewDto(
                TotalRevenueBeforeRefund: totalRevenueBeforeRefund,
                TotalTicketsSold: totalTicketsSold,
                NetRevenue: netRevenue,
                RefundOrderCount: refundOrderCount,
                TotalRefundAmount: totalRefundAmount,
                OccupancyRate: occupancyRate,
                AverageRevenuePerTicket: averageRevenuePerTicket),
            ByTicketType: byTicketType,
            ByTime: byTime,
            Profit: profit,
            Refunds: refunds,
            DiscountCodes: []);

        return Result.Success(response);
    }

    private static (DateTime StartUtc, DateTime EndUtc) GetCurrentPeriodRange(RevenueTimePeriod period, DateTime nowUtc)
    {
        var date = nowUtc.Date;

        return period switch
        {
            RevenueTimePeriod.Day =>
                (date, date.AddDays(1)),

            RevenueTimePeriod.Week =>
                GetWeekRange(date),

            RevenueTimePeriod.Month =>
                (new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                 new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),

            RevenueTimePeriod.Quarter =>
                GetQuarterRange(date),

            _ =>
                (date, date.AddDays(1))
        };
    }

    private static (DateTime StartUtc, DateTime EndUtc) GetWeekRange(DateTime dateUtc)
    {
        var diff = ((int)dateUtc.DayOfWeek + 6) % 7;
        var start = dateUtc.AddDays(-diff);
        return (start, start.AddDays(7));
    }

    private static (DateTime StartUtc, DateTime EndUtc) GetQuarterRange(DateTime dateUtc)
    {
        var quarterStartMonth = ((dateUtc.Month - 1) / 3) * 3 + 1;
        var start = new DateTime(dateUtc.Year, quarterStartMonth, 1, 0, 0, 0, DateTimeKind.Utc);
        return (start, start.AddMonths(3));
    }

    private static string BuildPeriodLabel(RevenueTimePeriod period, DateTime startUtc, DateTime endUtc)
    {
        return period switch
        {
            RevenueTimePeriod.Day => startUtc.ToString("yyyy-MM-dd"),
            RevenueTimePeriod.Week => $"{startUtc:yyyy-MM-dd} to {endUtc.AddDays(-1):yyyy-MM-dd}",
            RevenueTimePeriod.Month => startUtc.ToString("yyyy-MM"),
            RevenueTimePeriod.Quarter => $"Q{((startUtc.Month - 1) / 3) + 1}-{startUtc:yyyy}",
            _ => startUtc.ToString("yyyy-MM-dd")
        };
    }
}
