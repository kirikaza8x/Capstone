using Events.PublicApi.PublicApi;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Repositories;

namespace Ticketing.Application.Orders.Queries.GetCheckInStats;

internal sealed class GetCheckInStatsQueryHandler(
    IOrderRepository orderRepository,
    IEventTicketingPublicApi eventTicketingPublicApi)
    : IQueryHandler<GetCheckInStatsQuery, CheckInStatsResponse>
{
    public async Task<Result<CheckInStatsResponse>> Handle(
        GetCheckInStatsQuery query,
        CancellationToken cancellationToken)
    {
        // 1. Nhận về danh sách Tuple (TicketTypeId, Status)
        var tickets = await orderRepository.GetTicketStatsBySessionAsync(
            query.EventId,
            query.EventSessionId,
            cancellationToken);

        if (!tickets.Any())
        {
            return Result.Success(new CheckInStatsResponse(
                new CheckInSummary(0, 0),
                []));
        }

        // calculate summary
        var totalTickets = tickets.Count;
        var totalCheckedIn = tickets.Count(t => t.Status == OrderTicketStatus.Used);

        // get name of ticket types from public API
        var ticketTypeIds = tickets.Select(t => t.TicketTypeId).Distinct().ToList();
        var ticketTypeMap = await eventTicketingPublicApi.GetTicketTypeDetailsAsync(
            ticketTypeIds,
            cancellationToken);

        // group by TicketTypeId and calculate stats
        var stats = tickets
            .GroupBy(t => t.TicketTypeId)
            .Select(g =>
            {
                ticketTypeMap.TryGetValue(g.Key, out var detail);
                return new TicketTypeStat(
                    detail?.Name ?? "Unknown",
                    detail?.Quantity ?? 0,
                    g.Count(t => t.Status == OrderTicketStatus.Used)
                );
            })
            .ToList();

        var totalQuantity = ticketTypeMap.Values.Sum(detail => detail.Quantity);

        return Result.Success(new CheckInStatsResponse(
            new CheckInSummary(totalTickets, totalQuantity, totalCheckedIn),
            stats));
    }
}
