namespace Payment.PublicApi.PublicApi;

public interface IPaymentPublicApi
{
    Task<GlobalRevenueOverviewDto> GetGlobalRevenueOverviewAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyRevenueTrendDto>> GetGlobalSalesTrendAsync(
        DateTime startDateUtc,
        DateTime endDateUtc,
        CancellationToken cancellationToken = default);
}

public sealed record GlobalRevenueOverviewDto(
    decimal GrossRevenue,
    double MonthlyGrowthRate,
    bool IsPositiveGrowth);

public sealed record DailyRevenueTrendDto(
    DateTime Date,
    decimal Revenue,
    int Transactions);
