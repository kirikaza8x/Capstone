using Events.PublicApi.PublicApi;
using Events.PublicApi.Records;
using Payment.PublicApi.PublicApi;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Repositories;

namespace Ticketing.Application.Reports.GetOrganizerRevenueSummary;

internal sealed class GetOrganizerRevenueSummaryQueryHandler(
    IOrderRepository orderRepository,
    IEventTicketingPublicApi eventTicketingPublicApi,
    ICurrentUserService currentUserService,
    IPaymentPublicApi paymentPublicApi)
    : IQueryHandler<GetOrganizerRevenueSummaryQuery, OrganizerRevenueSummaryResponse>
{
    public async Task<Result<OrganizerRevenueSummaryResponse>> Handle(
        GetOrganizerRevenueSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var currentUserId = currentUserService.UserId;
        var eventIds = await eventTicketingPublicApi.GetEventIdsByUserIdAsync(currentUserId, cancellationToken);

        if (eventIds.Count == 0)
        {
            return Result.Success(new OrganizerRevenueSummaryResponse(
                currentUserId,
                0m,
                0m,
                0m,
                0m,
                0,
                0,
                0,
                0));
        }

        var overviews = await eventTicketingPublicApi.GetOrganizerEventOverviewByEventIdsAsync(eventIds, cancellationToken);

        decimal grossRevenue = 0m;
        decimal voucherDiscount = 0m;

        foreach (var eventId in eventIds)
        {
            var ticketTypes = await eventTicketingPublicApi.GetAllTicketTypesByEventIdAsync(eventId, cancellationToken);
            if (ticketTypes is null || !ticketTypes.Any())
            {
                continue;
            }

            var ticketTypeIds = ticketTypes.Select(t => t.Id).ToList();
            var paidOrders = await orderRepository.GetPaidOrdersByTicketTypeIdsAsync(ticketTypeIds, cancellationToken);

            foreach (var order in paidOrders)
            {
                var validTickets = order.Tickets
                    .Where(t => t.Status != OrderTicketStatus.Cancelled)
                    .ToList();

                if (validTickets.Count == 0)
                {
                    continue;
                }

                var orderGross = validTickets.Sum(t => t.Price);
                var orderNetBeforeRefund = Math.Min(order.TotalPrice, orderGross);

                grossRevenue += orderGross;
                voucherDiscount += Math.Max(0m, orderGross - orderNetBeforeRefund);
            }
        }

        var totalRefunds = await paymentPublicApi.GetTotalRefundsByEventIdsAsync(eventIds, cancellationToken);
        var netRevenue = Math.Max(0m, grossRevenue - voucherDiscount - totalRefunds);

        var now = DateTime.UtcNow;
        var completedEventCount = overviews.Values.Count(x => IsCompleted(x, now));
        var upcomingEventCount = overviews.Values.Count(x => IsUpcoming(x, now));
        var activeEventCount = overviews.Values.Count(x => IsActive(x, now));

        return Result.Success(new OrganizerRevenueSummaryResponse(
            currentUserId,
            grossRevenue,
            voucherDiscount,
            totalRefunds,
            netRevenue,
            overviews.Count,
            completedEventCount,
            activeEventCount,
            upcomingEventCount));
    }

    private static bool IsCompleted(OrganizerEventOverviewDto x, DateTime now) =>
        x.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
        (x.EventEndAt.HasValue && x.EventEndAt.Value <= now);

    private static bool IsUpcoming(OrganizerEventOverviewDto x, DateTime now) =>
        x.Status.Equals("Published", StringComparison.OrdinalIgnoreCase) &&
        x.EventStartAt.HasValue &&
        x.EventStartAt.Value > now;

    private static bool IsActive(OrganizerEventOverviewDto x, DateTime now) =>
        x.Status.Equals("Published", StringComparison.OrdinalIgnoreCase) &&
        (!x.EventStartAt.HasValue || x.EventStartAt.Value <= now) &&
        (!x.EventEndAt.HasValue || x.EventEndAt.Value > now);
}
