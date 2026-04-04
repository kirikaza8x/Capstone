using Payment.Application.Features.Vnpay.DTOs;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

public class GetNetRevenuePerEventQueryHandler(IPaymentTransactionRepository repo)
    : IQueryHandler<GetNetRevenuePerEventQuery, IReadOnlyList<EventRevenueDto>>
{
    public async Task<Result<IReadOnlyList<EventRevenueDto>>> Handle(
        GetNetRevenuePerEventQuery query,
        CancellationToken cancellationToken)
    {
        var data = await repo.GetNetRevenuePerEventAsync(cancellationToken);

        var dto = data
            .Select(x => new EventRevenueDto(x.EventId, x.Revenue))
            .ToList();

        return Result.Success<IReadOnlyList<EventRevenueDto>>(dto);
    }
}