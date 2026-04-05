using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Reports.GetSalesTrend;

public sealed record GetSalesTrendQuery(
    Guid EventId,
    DateTime StartDate,
    DateTime EndDate) : IQuery<SalesTrendResponse>;

public sealed record SalesTrendResponse(
    Guid EventId,
    DateTime StartDate,
    DateTime EndDate,
    IReadOnlyList<SalesTrendPoint> Trend);

public sealed record SalesTrendPoint(
    DateTime Time,
    int TicketsSold,
    decimal NetRevenue,
    decimal GrossRevenue);
