namespace Payments.PublicApi.PublicApi;

public interface IAiPackageRevenuePublicApi
{
    Task<AiPackageRevenueOverviewDto> GetAiPackageRevenueOverviewAsync(
        CancellationToken cancellationToken = default);
}

public sealed record AiPackageRevenueOverviewDto(
    decimal TotalRevenue,
    double MonthlyGrowthRate,
    bool IsPositiveGrowth,
    Guid? TopPackageId,
    int TopPackageOrganizationCount);
