using Events.PublicApi.PublicApi;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Repositories;

namespace Ticketing.Application.Reports.GetCheckInStats;

internal sealed class GetCheckInStatsQueryHandler(
    IOrderRepository orderRepository,
    IEventTicketingPublicApi eventTicketingPublicApi)
    : IQueryHandler<GetCheckInStatsQuery, CheckInStatsResponse>
{
    public async Task<Result<CheckInStatsResponse>> Handle(
        GetCheckInStatsQuery query,
        CancellationToken cancellationToken)
    {
        // Get all ticket types info
        var ticketTypes = await eventTicketingPublicApi.GetAllTicketTypesByEventIdAsync(
            query.EventId,
            cancellationToken);

        if (ticketTypes is null || !ticketTypes.Any())
        {
            return Result.Success(new CheckInStatsResponse(
                new CheckInSummary(0, 0, 0, 0, 0),
                []));
        }

        // get list tuple (TicketTypeId, Status)
        var tickets = await orderRepository.GetTicketStatsBySessionAsync(
            query.EventId,
            query.EventSessionId,
            cancellationToken);

        // filter active tickets
        var validTickets = tickets
            .Where(t => t.Status != OrderTicketStatus.Cancelled)
            .ToList();

        // Calculate summary data
        int totalTicketTypes = ticketTypes.Count;
        int totalTicketQuantity = ticketTypes.Sum(t => t.Quantity);
        int totalSold = validTickets.Count;
        int totalCheckedIn = validTickets.Count(t => t.Status == OrderTicketStatus.Used);

        // calculate check-in rate, round to 2 decimal places
        double checkInRate = totalSold > 0
            ? Math.Round((double)totalCheckedIn / totalSold * 100, 2)
            : 0;

        var summary = new CheckInSummary(
            totalTicketTypes,
            totalTicketQuantity,
            totalSold,
            totalCheckedIn,
            checkInRate);

        // Geoup tickets by TicketTypeId to calculate sold and checked-in count for each type
        var soldTicketsGroup = validTickets
            .GroupBy(t => t.TicketTypeId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Sold = g.Count(),
                    CheckedIn = g.Count(t => t.Status == OrderTicketStatus.Used)
                });

        // Browse through all ticket types to create a detailed statistical array
        var stats = ticketTypes.Select(tt =>
        {
            var hasSales = soldTicketsGroup.TryGetValue(tt.Id, out var salesData);

            return new TicketTypeStat(
                tt.Id,
                tt.Name,
                tt.Quantity,
                hasSales ? salesData.Sold : 0,           
                hasSales ? salesData.CheckedIn : 0      
            );
        }).ToList();

        return Result.Success(new CheckInStatsResponse(summary, stats));
    }
}
