using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

public class GetRefundRateByEventQueryHandler(IPaymentTransactionRepository repo)
    : IQueryHandler<GetRefundRateByEventQuery, EventRefundRateDto>
{
    public async Task<Result<EventRefundRateDto>> Handle(
        GetRefundRateByEventQuery query, CancellationToken cancellationToken)
    {
        var result = await repo.GetRefundRateByEventAsync(query.EventId, cancellationToken);

        if (result is null)
            return Result.Failure<EventRefundRateDto>(
                Error.NotFound("RefundRate.NotFound", "No completed transactions for this event."));

        return Result.Success(new EventRefundRateDto(
            result.EventId,
            result.GrossRevenue,
            result.TotalRefunds,
            result.RefundRatePercent));
    }
}