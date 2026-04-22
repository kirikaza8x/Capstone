using Payment.PublicApi.PublicApi;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Time;
using Shared.Domain.Abstractions;

namespace Reports.Application.Admin.Queries.GetFundFlow;

internal sealed class GetFundFlowQueryHandler(
    IPaymentPublicApi paymentPublicApi,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetFundFlowQuery, FundFlowResponse>
{
    public async Task<Result<FundFlowResponse>> Handle(
        GetFundFlowQuery query,
        CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;

        var (currentStart, currentEnd, previousStart, previousEnd) = GetRanges(query.Period, now);

        var currentSummary = await paymentPublicApi.GetFundFlowSummaryAsync(
            currentStart,
            currentEnd,
            cancellationToken);

        var previousSummary = await paymentPublicApi.GetFundFlowSummaryAsync(
            previousStart,
            previousEnd,
            cancellationToken);

        var current = new FundFlowBreakdownDto(
            TicketPurchase: currentSummary.TicketPurchaseAmount,
            AiPackagePurchase: currentSummary.AiPackagePurchaseAmount,
            WalletTopUp: currentSummary.WalletTopUpAmount,
            Refund: currentSummary.RefundAmount,
            Withdrawal: currentSummary.WithdrawalAmount);

        var previous = new FundFlowBreakdownDto(
            TicketPurchase: previousSummary.TicketPurchaseAmount,
            AiPackagePurchase: previousSummary.AiPackagePurchaseAmount,
            WalletTopUp: previousSummary.WalletTopUpAmount,
            Refund: previousSummary.RefundAmount,
            Withdrawal: previousSummary.WithdrawalAmount);

        var comparison = new FundFlowComparisonDto(
            TicketPurchase: BuildMetricComparison(current.TicketPurchase, previous.TicketPurchase),
            AiPackagePurchase: BuildMetricComparison(current.AiPackagePurchase, previous.AiPackagePurchase),
            WalletTopUp: BuildMetricComparison(current.WalletTopUp, previous.WalletTopUp),
            Refund: BuildMetricComparison(current.Refund, previous.Refund),
            Withdrawal: BuildMetricComparison(current.Withdrawal, previous.Withdrawal));

        var response = new FundFlowResponse(
            Period: query.Period.ToString().ToLowerInvariant(),
            CurrentPeriodStartUtc: currentStart,
            CurrentPeriodEndUtc: currentEnd,
            Current: current,
            Previous: previous,
            Comparison: comparison);

        return Result.Success(response);
    }

    private static MetricComparisonDto BuildMetricComparison(decimal current, decimal previous)
    {
        var difference = current - previous;

        var changeRate = previous == 0m
            ? (current > 0m ? 100d : 0d)
            : Math.Round((double)(difference / previous * 100m), 2);

        return new MetricComparisonDto(
            CurrentValue: current,
            PreviousValue: previous,
            Difference: difference,
            ChangeRate: changeRate,
            IsPositiveGrowth: changeRate >= 0d);
    }

    private static (DateTime CurrentStart, DateTime CurrentEnd, DateTime PreviousStart, DateTime PreviousEnd) GetRanges(
        FundFlowPeriod period,
        DateTime nowUtc)
    {
        var nowDate = nowUtc.Date;

        return period switch
        {
            FundFlowPeriod.Week => GetWeekRanges(nowDate),
            FundFlowPeriod.Quarter => GetQuarterRanges(nowDate),
            _ => GetMonthRanges(nowDate)
        };
    }

    private static (DateTime CurrentStart, DateTime CurrentEnd, DateTime PreviousStart, DateTime PreviousEnd) GetWeekRanges(
        DateTime nowDate)
    {
        var dayOffset = ((int)nowDate.DayOfWeek + 6) % 7;
        var currentStart = nowDate.AddDays(-dayOffset);
        var currentEnd = currentStart.AddDays(7);
        var previousStart = currentStart.AddDays(-7);
        var previousEnd = currentStart;

        return (currentStart, currentEnd, previousStart, previousEnd);
    }

    private static (DateTime CurrentStart, DateTime CurrentEnd, DateTime PreviousStart, DateTime PreviousEnd) GetMonthRanges(
        DateTime nowDate)
    {
        var currentStart = new DateTime(nowDate.Year, nowDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentEnd = currentStart.AddMonths(1);
        var previousStart = currentStart.AddMonths(-1);
        var previousEnd = currentStart;

        return (currentStart, currentEnd, previousStart, previousEnd);
    }

    private static (DateTime CurrentStart, DateTime CurrentEnd, DateTime PreviousStart, DateTime PreviousEnd) GetQuarterRanges(
        DateTime nowDate)
    {
        var quarterStartMonth = ((nowDate.Month - 1) / 3) * 3 + 1;
        var currentStart = new DateTime(nowDate.Year, quarterStartMonth, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentEnd = currentStart.AddMonths(3);
        var previousStart = currentStart.AddMonths(-3);
        var previousEnd = currentStart;

        return (currentStart, currentEnd, previousStart, previousEnd);
    }
}
