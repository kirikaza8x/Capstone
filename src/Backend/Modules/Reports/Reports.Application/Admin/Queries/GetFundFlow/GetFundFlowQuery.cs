using Shared.Application.Abstractions.Messaging;

namespace Reports.Application.Admin.Queries.GetFundFlow;

public enum FundFlowPeriod
{
    Week,
    Month,
    Quarter
}

public sealed record GetFundFlowQuery(FundFlowPeriod Period = FundFlowPeriod.Month)
    : IQuery<FundFlowResponse>;

public sealed record FundFlowResponse(
    string Period,
    DateTime CurrentPeriodStartUtc,
    DateTime CurrentPeriodEndUtc,
    FundFlowBreakdownDto Current,
    FundFlowBreakdownDto Previous,
    FundFlowComparisonDto Comparison);

public sealed record FundFlowBreakdownDto(
    decimal TicketPurchase,
    decimal AiPackagePurchase,
    decimal WalletTopUp,
    decimal Refund,
    decimal Withdrawal);

public sealed record FundFlowComparisonDto(
    MetricComparisonDto TicketPurchase,
    MetricComparisonDto AiPackagePurchase,
    MetricComparisonDto WalletTopUp,
    MetricComparisonDto Refund,
    MetricComparisonDto Withdrawal);

public sealed record MetricComparisonDto(
    decimal CurrentValue,
    decimal PreviousValue,
    decimal Difference,
    double ChangeRate,
    bool IsPositiveGrowth);
