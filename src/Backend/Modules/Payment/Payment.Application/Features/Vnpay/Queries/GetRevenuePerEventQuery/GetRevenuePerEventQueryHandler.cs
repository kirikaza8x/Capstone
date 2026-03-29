using Payments.Application.Features.Vnpay.DTOs;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

public class GetRevenuePerEventQueryHandler(
    IPaymentTransactionRepository repo)
    : IQueryHandler<GetRevenuePerEventQuery, IReadOnlyList<EventRevenueDto>>
{
    public async Task<Result<IReadOnlyList<EventRevenueDto>>> Handle(
        GetRevenuePerEventQuery query,
        CancellationToken cancellationToken)
    {
        var data = await repo.GetRevenuePerEventAsync(cancellationToken);

        var dto = data
            .Select(x => new EventRevenueDto(
                x.EventId,
                x.Revenue))
            .ToList();

        return Result.Success<IReadOnlyList<EventRevenueDto>>(dto);
    }
}