using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Reports.GetSalesTrend;

public enum SalesTrendPeriod
{
    Day,
    Week
}

public sealed record GetSalesTrendQuery(Guid EventId, SalesTrendPeriod Period) : IQuery<SalesTrendResponse>;

public sealed record SalesTrendResponse(
    Guid EventId,
    string Period,
    IReadOnlyList<SalesTrendPoint> Trend);

public sealed record SalesTrendPoint(
    string TimeLabel,
    int TicketsSold,
    decimal Revenue);
