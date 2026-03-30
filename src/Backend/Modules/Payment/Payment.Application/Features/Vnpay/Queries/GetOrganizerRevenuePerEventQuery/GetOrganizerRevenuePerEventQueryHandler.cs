using Events.PublicApi.PublicApi;
using Payments.Application.Features.Vnpay.DTOs;
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
            query.OrganizerId, cancellationToken);

        if (eventIds.Count == 0)
            return Result.Success(new OrganizerRevenuePerEventDto(
                query.OrganizerId, []));

        var rows = query.ByNet
            ? await repo.GetNetRevenueByEventIdsAsync(eventIds, cancellationToken)
            : await repo.GetRevenueByEventIdsAsync(eventIds, cancellationToken);

        var dto = rows
            .Select(x => new EventRevenueDto(x.EventId, x.Revenue))
            .ToList();

        return Result.Success(new OrganizerRevenuePerEventDto(query.OrganizerId, dto));
    }
}