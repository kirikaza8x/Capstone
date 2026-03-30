using Payments.Application.Features.Vnpay.DTOs;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

public class GetNetRevenueByEventQueryHandler(
    IPaymentTransactionRepository repo)
    : IQueryHandler<GetNetRevenueByEventQuery, EventRevenueDto>
{
    public async Task<Result<EventRevenueDto>> Handle(
        GetNetRevenueByEventQuery query,
        CancellationToken cancellationToken)
    {
        var netRevenue = await repo.GetNetRevenueByEventAsync(
            query.EventId, cancellationToken);

        var dto = new EventRevenueDto(
            query.EventId,
            netRevenue);

        return Result.Success(dto);
    }
}