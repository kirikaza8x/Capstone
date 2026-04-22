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

        var validReferenceTypes = new[]
        {
            PaymentReferenceType.TicketOrder,
            PaymentReferenceType.AiPackage,
        };

        var baseQuery = dbContext.PaymentTransactions
            .AsNoTracking()
            .Where(x =>
                x.InternalStatus == PaymentInternalStatus.Completed &&
                (x.ReferenceType == PaymentReferenceType.TicketOrder ||
                 x.ReferenceType == PaymentReferenceType.AiPackage));

        var netRevenue = await baseQuery
            .Select(x =>
                x.Amount -
                x.Items
                    .Where(i => i.InternalStatus == PaymentInternalStatus.Refunded)
                    .Sum(i => i.Amount))
            .SumAsync(cancellationToken);

        var currentMonthNetRevenue = await baseQuery
            .Where(x =>
                (x.CompletedAt ?? x.CreatedAt ?? DateTime.MinValue) >= currentMonthStart &&
                (x.CompletedAt ?? x.CreatedAt ?? DateTime.MinValue) < now)
            .Select(x =>
                x.Amount -
                x.Items
                    .Where(i => i.InternalStatus == PaymentInternalStatus.Refunded)
                    .Sum(i => i.Amount))
            .SumAsync(cancellationToken);

        var previousMonthNetRevenue = await baseQuery
            .Where(x =>
                (x.CompletedAt ?? x.CreatedAt ?? DateTime.MinValue) >= previousMonthStart &&
                (x.CompletedAt ?? x.CreatedAt ?? DateTime.MinValue) < currentMonthStart)
            .Select(x =>
                x.Amount -
                x.Items
                    .Where(i => i.InternalStatus == PaymentInternalStatus.Refunded)
                    .Sum(i => i.Amount))
            .SumAsync(cancellationToken);

        var monthlyGrowthRate = previousMonthNetRevenue == 0m
            ? (currentMonthNetRevenue > 0m ? 100d : 0d)
            : (double)((currentMonthNetRevenue - previousMonthNetRevenue) / previousMonthNetRevenue * 100m);

        return new GlobalRevenueOverviewDto(
            NetRevenue: netRevenue,
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

    public async Task<FundFlowSummaryDto> GetFundFlowSummaryAsync(
    DateTime? startDateUtc = null,
    DateTime? endDateUtc = null,
    CancellationToken cancellationToken = default)
    {
        var paymentTransactions = dbContext.PaymentTransactions
            .AsNoTracking()
            .Where(x => x.InternalStatus == PaymentInternalStatus.Completed);

        if (startDateUtc.HasValue)
        {
            paymentTransactions = paymentTransactions.Where(x =>
                (x.CompletedAt ?? x.CreatedAt ?? DateTime.MinValue) >= startDateUtc.Value);
        }

        if (endDateUtc.HasValue)
        {
            paymentTransactions = paymentTransactions.Where(x =>
                (x.CompletedAt ?? x.CreatedAt ?? DateTime.MinValue) < endDateUtc.Value);
        }

        var walletTransactions = dbContext.WalletTransactions
            .AsNoTracking()
            .Where(x => x.Status == TransactionStatus.Completed);

        if (startDateUtc.HasValue)
        {
            walletTransactions = walletTransactions.Where(x =>
                (x.CreatedAt ?? DateTime.MinValue) >= startDateUtc.Value);
        }

        if (endDateUtc.HasValue)
        {
            walletTransactions = walletTransactions.Where(x =>
                (x.CreatedAt ?? DateTime.MinValue) < endDateUtc.Value);
        }

        var ticketPurchaseAmount = await paymentTransactions
            .Where(x => x.ReferenceType == PaymentReferenceType.TicketOrder)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var aiPackagePurchaseAmount = await paymentTransactions
            .Where(x => x.ReferenceType == PaymentReferenceType.AiPackage)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var walletTopUpAmount = await paymentTransactions
            .Where(x => x.Type == PaymentType.WalletTopUp)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var refundAmount = await walletTransactions
            .Where(x => x.Type == TransactionType.Refund)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var withdrawalAmount = await walletTransactions
            .Where(x => x.Type == TransactionType.Withdrawal)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        return new FundFlowSummaryDto(
            TicketPurchaseAmount: ticketPurchaseAmount,
            AiPackagePurchaseAmount: aiPackagePurchaseAmount,
            WalletTopUpAmount: walletTopUpAmount,
            RefundAmount: refundAmount,
            WithdrawalAmount: withdrawalAmount);
    }
}
