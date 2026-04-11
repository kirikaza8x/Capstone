using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

public class GetTotalRefundsByEventQueryHandler(IPaymentTransactionRepository repo)
    : IQueryHandler<GetTotalRefundsByEventQuery, decimal>
{
    public async Task<Result<decimal>> Handle(
        GetTotalRefundsByEventQuery query, CancellationToken cancellationToken)
    {
        var refunds = await repo.GetTotalRefundsByEventAsync(query.EventId, cancellationToken);
        return Result.Success(refunds);
    }
}