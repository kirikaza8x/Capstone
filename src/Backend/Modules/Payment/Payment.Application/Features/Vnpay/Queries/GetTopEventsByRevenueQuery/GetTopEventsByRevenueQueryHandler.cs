using Payment.Application.Features.Vnpay.DTOs;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

public class GetTopEventsByRevenueQueryHandler(IPaymentTransactionRepository repo)
    : IQueryHandler<GetTopEventsByRevenueQuery, IReadOnlyList<EventRevenueDto>>
{
    public async Task<Result<IReadOnlyList<EventRevenueDto>>> Handle(
        GetTopEventsByRevenueQuery query, CancellationToken cancellationToken)
    {
        var data = await repo.GetTopEventsByRevenueAsync(
            query.TopN, query.ByNet, cancellationToken);

        var dto = data
            .Select(x => new EventRevenueDto(x.EventId, x.Revenue))
            .ToList();

        return Result.Success<IReadOnlyList<EventRevenueDto>>(dto);
    }
}