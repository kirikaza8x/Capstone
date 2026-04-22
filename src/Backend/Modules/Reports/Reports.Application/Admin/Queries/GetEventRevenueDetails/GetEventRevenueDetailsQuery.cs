using Shared.Application.Abstractions.Messaging;

namespace Reports.Application.Admin.Queries.GetEventRevenueDetails;

public enum RevenueTimePeriod
{
    Day,
    Week,
    Month,
    Quarter
}

public sealed record GetEventRevenueDetailsQuery(
    Guid EventId,
    RevenueTimePeriod Period = RevenueTimePeriod.Week)
    : IQuery<EventRevenueDetailsResponse>;

public sealed record EventRevenueDetailsResponse(
    RevenueOverviewDto Overview,
    IReadOnlyList<TicketTypeRevenueDto> ByTicketType,
    IReadOnlyList<RevenueByTimeDto> ByTime,
    ProfitSummaryDto Profit,
    RefundSummaryDto Refunds,
    IReadOnlyList<DiscountCodeRevenueDto> DiscountCodes);

public sealed record RevenueOverviewDto(
    decimal TotalRevenueBeforeRefund,
    int TotalTicketsSold,
    decimal NetRevenue,
    int RefundOrderCount,
    decimal TotalRefundAmount,
    double OccupancyRate,
    decimal AverageRevenuePerTicket);

public sealed record TicketTypeRevenueDto(
    string TicketTypeName,
    decimal ListedPrice,
    decimal DiscountedPrice,
    int IssuedQuantity,
    int SoldQuantity,
    int CancelledOrRefundedQuantity,
    decimal Revenue,
    double ContributionRate,
    string Status);

public sealed record RevenueByTimeDto(
    string TimeLabel,
    int TicketsSoldInPeriod,
    decimal RevenueInPeriod);

public sealed record ProfitSummaryDto(
    decimal GrossProfit,
    double ProfitMargin);

public sealed record RefundSummaryDto(
    int RefundOrderCount,
    decimal TotalRefundAmount,
    double RefundRate);

public sealed record DiscountCodeRevenueDto(
    string Code,
    int UsageCount,
    decimal TotalDiscountAmount,
    decimal NetRevenueAfterDiscount);
