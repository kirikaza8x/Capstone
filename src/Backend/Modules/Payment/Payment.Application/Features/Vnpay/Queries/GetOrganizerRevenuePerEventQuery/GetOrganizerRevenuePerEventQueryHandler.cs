using Events.PublicApi.PublicApi;
using Payment.Application.Features.Vnpay.DTOs;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

public class GetOrganizerRevenuePerEventQueryHandler(
    IPaymentTransactionRepository repo,
    IEventTicketingPublicApi eventApi)
    : IQueryHandler<GetOrganizerRevenuePerEventQuery, OrganizerRevenuePerEventDto>
{
    public async Task<Result<OrganizerRevenuePerEventDto>> Handle(
        GetOrganizerRevenuePerEventQuery query,
        CancellationToken cancellationToken)
    {
        var eventIds = await eventApi.GetEventIdsByUserIdAsync(
            query.OrganizerId,
            cancellationToken);

        if (eventIds.Count == 0)
            return Result.Success(new OrganizerRevenuePerEventDto(query.OrganizerId, []));

        var overviews = await eventApi.GetOrganizerEventOverviewByEventIdsAsync(eventIds, cancellationToken);

        var grossRows = await repo.GetRevenueByEventIdsAsync(eventIds, cancellationToken);
        var netRows = await repo.GetNetRevenueByEventIdsAsync(eventIds, cancellationToken);

        var grossMap = grossRows.ToDictionary(x => x.EventId, x => x.Revenue);
        var netMap = netRows.ToDictionary(x => x.EventId, x => x.Revenue);

        IReadOnlyList<OrganizerRevenuePerEventItemDto> perEvent = overviews.Values
            .Select(x =>
            {
                var grossRevenue = grossMap.GetValueOrDefault(x.EventId, 0m);
                var netRevenue = netMap.GetValueOrDefault(x.EventId, 0m);
                var refundAmount = Math.Max(0m, grossRevenue - netRevenue);
                var refundRate = grossRevenue > 0m
                    ? Math.Round(refundAmount / grossRevenue * 100m, 2)
                    : 0m;

                return new OrganizerRevenuePerEventItemDto(
                    x.EventId,
                    x.EventName,
                    grossRevenue,
                    netRevenue,
                    refundAmount,
                    refundRate,
                    x.Status);
            })
            .OrderByDescending(x => query.ByNet ? x.NetRevenue : x.GrossRevenue)
            .ToList();

        return Result.Success(new OrganizerRevenuePerEventDto(query.OrganizerId, perEvent));
    }
}
