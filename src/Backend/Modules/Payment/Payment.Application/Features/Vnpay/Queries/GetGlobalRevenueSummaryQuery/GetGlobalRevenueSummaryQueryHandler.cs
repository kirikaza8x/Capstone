using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

public class GetGlobalRevenueSummaryQueryHandler(IPaymentTransactionRepository repo)
    : IQueryHandler<GetGlobalRevenueSummaryQuery, GlobalRevenueSummaryDto>
{
    public async Task<Result<GlobalRevenueSummaryDto>> Handle(
        GetGlobalRevenueSummaryQuery query, CancellationToken cancellationToken)
    {
        var result = await repo.GetGlobalRevenueSummaryAsync(cancellationToken);

        return Result.Success(new GlobalRevenueSummaryDto(
            result.GrossRevenue,
            result.TotalRefunds,
            result.NetRevenue,
            result.EventCount));
    }
}