using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

public class GetTransactionSummaryByEventQueryHandler(IPaymentTransactionRepository repo)
    : IQueryHandler<GetTransactionSummaryByEventQuery, EventTransactionSummaryDto>
{
    public async Task<Result<EventTransactionSummaryDto>> Handle(
        GetTransactionSummaryByEventQuery query, CancellationToken cancellationToken)
    {
        var result = await repo.GetTransactionSummaryByEventAsync(query.EventId, cancellationToken);

        if (result is null)
            return Result.Failure<EventTransactionSummaryDto>(
                Error.NotFound("TransactionSummary.NotFound", "No transactions found for this event."));

        return Result.Success(new EventTransactionSummaryDto(
            result.EventId,
            result.TotalTransactions,
            result.CompletedCount,
            result.FailedCount,
            result.RefundedCount,
            result.WalletPayAmount,
            result.DirectPayAmount));
    }
}