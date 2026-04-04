using Events.PublicApi.PublicApi;
using Events.PublicApi.Records;
using Payment.Application.Features.Vnpay.DTOs;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

public class GetOrganizerRevenueSummaryQueryHandler(
    IPaymentTransactionRepository repo,
    IEventTicketingPublicApi eventApi)
    : IQueryHandler<GetOrganizerRevenueSummaryQuery, OrganizerRevenueSummaryDto>
{
    public async Task<Result<OrganizerRevenueSummaryDto>> Handle(
        GetOrganizerRevenueSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var eventIds = await eventApi.GetEventIdsByUserIdAsync(query.OrganizerId, cancellationToken);

        if (eventIds.Count == 0)
        {
            return Result.Success(new OrganizerRevenueSummaryDto(
                query.OrganizerId,
                0m,
                0m,
                0m,
                0,
                0,
                0,
                0));
        }

        var summary = await repo.GetRevenueSummaryByEventIdsAsync(
            eventIds,
            cancellationToken);

        var overviews = await eventApi.GetOrganizerEventOverviewByEventIdsAsync(
            eventIds,
            cancellationToken);

        var now = DateTime.UtcNow;
        var completedEventCount = overviews.Values.Count(x => IsCompleted(x, now));
        var upcomingEventCount = overviews.Values.Count(x => IsUpcoming(x, now));
        var activeEventCount = overviews.Values.Count(x => IsActive(x, now));

        return Result.Success(new OrganizerRevenueSummaryDto(
            query.OrganizerId,
            summary.GrossRevenue,
            summary.TotalRefunds,
            summary.NetRevenue,
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
