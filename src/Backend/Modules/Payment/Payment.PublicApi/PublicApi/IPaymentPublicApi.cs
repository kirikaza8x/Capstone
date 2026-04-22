namespace Payment.PublicApi.PublicApi;

public interface IPaymentPublicApi
{
    Task<GlobalRevenueOverviewDto> GetGlobalRevenueOverviewAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyRevenueTrendDto>> GetGlobalSalesTrendAsync(
        DateTime startDateUtc,
        DateTime endDateUtc,
        CancellationToken cancellationToken = default);

    Task<FundFlowSummaryDto> GetFundFlowSummaryAsync(
        DateTime? startDateUtc = null,
        DateTime? endDateUtc = null,
        CancellationToken cancellationToken = default);
}

public sealed record GlobalRevenueOverviewDto(
    decimal NetRevenue,
    double MonthlyGrowthRate,
    bool IsPositiveGrowth);

public sealed record DailyRevenueTrendDto(
    DateTime Date,
    decimal Revenue,
    int Transactions);

public sealed record FundFlowSummaryDto(
    decimal TicketPurchaseAmount,
    decimal AiPackagePurchaseAmount,
    decimal WalletTopUpAmount,
    decimal RefundAmount,
    decimal WithdrawalAmount);
