using Microsoft.EntityFrameworkCore;
using Payment.Domain.Enums;
using Payment.PublicApi.PublicApi;
using Payments.Infrastructure.Persistence.Contexts;

namespace Payment.Infrastructure.PublicApi;

internal sealed class PaymentPublicApi(PaymentModuleDbContext dbContext) : IPaymentPublicApi
{
    public async Task<GlobalRevenueOverviewDto> GetGlobalRevenueOverviewAsync(
       CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var previousMonthStart = currentMonthStart.AddMonths(-1);

        var validTypes = new[]
        {
            PaymentType.BatchDirectPay,
            PaymentType.BatchWalletPay
        };

        var baseQuery = dbContext.PaymentTransactions
            .AsNoTracking()
            .Where(x =>
                x.InternalStatus == PaymentInternalStatus.Completed &&
                validTypes.Contains(x.Type)
            );

        var grossRevenue = await baseQuery
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

        return new GlobalRevenueOverviewDto(
            GrossRevenue: grossRevenue,
            MonthlyGrowthRate: Math.Round(monthlyGrowthRate, 2),
            IsPositiveGrowth: monthlyGrowthRate >= 0d);
    }

    public async Task<IReadOnlyList<DailyRevenueTrendDto>> GetGlobalSalesTrendAsync(
        DateTime startDateUtc,
        DateTime endDateUtc,
        CancellationToken cancellationToken = default)
    {
        var normalizedStart = DateTime.SpecifyKind(startDateUtc.Date, DateTimeKind.Utc);
        var normalizedEndExclusive = DateTime.SpecifyKind(endDateUtc.Date.AddDays(1), DateTimeKind.Utc);

        var rawData = await dbContext.PaymentTransactions
            .AsNoTracking()
            .Where(x =>
                x.InternalStatus == PaymentInternalStatus.Completed &&
                (x.CompletedAt ?? x.CreatedAt ?? DateTime.MinValue) >= normalizedStart &&
                (x.CompletedAt ?? x.CreatedAt ?? DateTime.MinValue) < normalizedEndExclusive)
            .GroupBy(x => (x.CompletedAt ?? x.CreatedAt ?? DateTime.MinValue).Date)
            .Select(g => new
            {
                Date = g.Key,
                Revenue = g.Sum(x => x.Amount),
                Transactions = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return rawData
            .Select(x => new DailyRevenueTrendDto(
                DateTime.SpecifyKind(x.Date, DateTimeKind.Utc),
                x.Revenue,
                x.Transactions))
            .ToList();
    }
}
