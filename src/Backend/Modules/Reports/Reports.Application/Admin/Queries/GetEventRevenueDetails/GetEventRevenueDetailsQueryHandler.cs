using Events.PublicApi.PublicApi;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.PublicApi;

namespace Reports.Application.Admin.Queries.GetEventRevenueDetails;

internal sealed class GetEventRevenueDetailsQueryHandler(
    IEventPublicApi eventPublicApi,
    IEventTicketingPublicApi eventTicketingPublicApi,
    ITicketingPublicApi ticketingPublicApi)
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
                TimeLabel: "All time",
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
}
