using Microsoft.EntityFrameworkCore;
using Payment.Domain.Enums;
using Payments.Domain.Enums;
using Payments.Infrastructure.Persistence.Contexts;
using Payments.PublicApi.PublicApi;

namespace Payments.Infrastructure.PublicApi;

internal sealed class AiPackageRevenuePublicApi(PaymentModuleDbContext dbContext) : IAiPackageRevenuePublicApi
{
    public async Task<AiPackageRevenueOverviewDto> GetAiPackageRevenueOverviewAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var previousMonthStart = currentMonthStart.AddMonths(-1);

        var baseQuery = dbContext.PaymentTransactions
            .AsNoTracking()
            .Where(x =>
                x.InternalStatus == PaymentInternalStatus.Completed &&
                x.ReferenceType == PaymentReferenceType.AiPackage &&
                x.ReferenceId.HasValue);

        var totalRevenue = await baseQuery
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var currentMonthRevenue = await baseQuery
            .Where(x =>
                (x.CompletedAt ?? x.CreatedAt ?? DateTime.MinValue) >= currentMonthStart &&
                (x.CompletedAt ?? x.CreatedAt ?? DateTime.MinValue) < now)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var previousMonthRevenue = await baseQuery
            .Where(x =>
                (x.CompletedAt ?? x.CreatedAt ?? DateTime.MinValue) >= previousMonthStart &&
                (x.CompletedAt ?? x.CreatedAt ?? DateTime.MinValue) < currentMonthStart)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var monthlyGrowthRate = previousMonthRevenue == 0m
            ? (currentMonthRevenue > 0m ? 100d : 0d)
            : (double)((currentMonthRevenue - previousMonthRevenue) / previousMonthRevenue * 100m);

        var topPackage = await baseQuery
            .GroupBy(x => x.ReferenceId!.Value)
            .Select(g => new
            {
                PackageId = g.Key,
                OrganizationCount = g.Select(x => x.UserId).Distinct().Count(),
                Revenue = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.OrganizationCount)
            .ThenByDescending(x => x.Revenue)
            .FirstOrDefaultAsync(cancellationToken);

        return new AiPackageRevenueOverviewDto(
            TotalRevenue: totalRevenue,
            MonthlyGrowthRate: Math.Round(monthlyGrowthRate, 2),
            IsPositiveGrowth: monthlyGrowthRate >= 0d,
            TopPackageId: topPackage?.PackageId,
            TopPackageOrganizationCount: topPackage?.OrganizationCount ?? 0);
    }
}
