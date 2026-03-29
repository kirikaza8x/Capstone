using Events.PublicApi.PublicApi;
using Payments.Application.Features.Vnpay.DTOs;
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
        // Step 1: resolve event IDs from Events module
        var eventIds = await eventApi.GetEventIdsByUserIdAsync(
            query.OrganizerId, cancellationToken);

        if (eventIds.Count == 0)
            return Result.Success(new OrganizerRevenueSummaryDto(
                query.OrganizerId, 0m, 0m, 0m, 0));

        // Step 2: query payment data for those event IDs
        var summary = await repo.GetRevenueSummaryByEventIdsAsync(
            eventIds, cancellationToken);

        return Result.Success(new OrganizerRevenueSummaryDto(
            query.OrganizerId,
            summary.GrossRevenue,
            summary.TotalRefunds,
            summary.NetRevenue,
            summary.EventCount));
    }
}