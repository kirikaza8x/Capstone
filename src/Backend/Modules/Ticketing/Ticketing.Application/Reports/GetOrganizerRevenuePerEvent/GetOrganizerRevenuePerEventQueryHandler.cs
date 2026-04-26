using Events.PublicApi.PublicApi;
using Payment.PublicApi.PublicApi;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Repositories;

namespace Ticketing.Application.Reports.GetOrganizerRevenuePerEvent;

internal sealed class GetOrganizerRevenuePerEventQueryHandler(
    IOrderRepository orderRepository,
    IEventTicketingPublicApi eventTicketingPublicApi,
    ICurrentUserService currentUserService,
    IPaymentPublicApi paymentPublicApi)
    : IQueryHandler<GetOrganizerRevenuePerEventQuery, OrganizerRevenuePerEventResponse>
{
    public async Task<Result<OrganizerRevenuePerEventResponse>> Handle(
        GetOrganizerRevenuePerEventQuery query,
        CancellationToken cancellationToken)
    {
        var currentUserId = currentUserService.UserId;
        var eventIds = await eventTicketingPublicApi.GetEventIdsByUserIdAsync(currentUserId, cancellationToken);

        if (eventIds.Count == 0)
        {
            return Result.Success(new OrganizerRevenuePerEventResponse(currentUserId, []));
        }

        var overviews = await eventTicketingPublicApi.GetOrganizerEventOverviewByEventIdsAsync(eventIds, cancellationToken);
        var refundsByEvent = await paymentPublicApi.GetRefundsByEventIdsAsync(eventIds, cancellationToken);

        var perEvent = new List<OrganizerRevenuePerEventItemResponse>();

        foreach (var eventId in eventIds)
        {
            var ticketTypes = await eventTicketingPublicApi.GetAllTicketTypesByEventIdAsync(eventId, cancellationToken);
            if (ticketTypes is null || !ticketTypes.Any())
            {
                perEvent.Add(BuildEmptyItem(eventId, overviews));
                continue;
            }

            var ticketTypeIds = ticketTypes.Select(t => t.Id).ToList();
            var paidOrders = await orderRepository.GetPaidOrdersByTicketTypeIdsAsync(ticketTypeIds, cancellationToken);

            decimal grossRevenue = 0m;
            decimal voucherDiscount = 0m;

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

            var refundAmount = refundsByEvent.GetValueOrDefault(eventId, 0m);
            var netRevenue = Math.Max(0m, grossRevenue - voucherDiscount - refundAmount);
            var discountRate = grossRevenue > 0m
                ? Math.Round(voucherDiscount / grossRevenue * 100m, 2)
                : 0m;

            var overview = overviews.GetValueOrDefault(eventId);

            perEvent.Add(new OrganizerRevenuePerEventItemResponse(
                eventId,
                overview?.EventName ?? string.Empty,
                grossRevenue,
                voucherDiscount,
                refundAmount,
                netRevenue,
                discountRate,
                overview?.Status ?? string.Empty));
        }

        var ordered = perEvent
            .OrderByDescending(x => query.ByNet ? x.NetRevenue : x.GrossRevenue)
            .ToList();

        return Result.Success(new OrganizerRevenuePerEventResponse(currentUserId, ordered));
    }

    private static OrganizerRevenuePerEventItemResponse BuildEmptyItem(
        Guid eventId,
        IReadOnlyDictionary<Guid, Events.PublicApi.Records.OrganizerEventOverviewDto> overviews)
    {
        var overview = overviews.GetValueOrDefault(eventId);

        return new OrganizerRevenuePerEventItemResponse(
            eventId,
            overview?.EventName ?? string.Empty,
            0m,
            0m,
            0m,
            0m,
            0m,
            overview?.Status ?? string.Empty);
    }
}
