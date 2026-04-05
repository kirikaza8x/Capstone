using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Reports.GetSalesTrendForAllEvent;

public sealed record GetSalesTrendForAllEventQuery(
    DateTime StartDate,
    DateTime EndDate) : IQuery<SalesTrendForAllEventResponse>;

public sealed record SalesTrendForAllEventResponse(
    Guid OrganizerId,
    DateTime StartDate,
    DateTime EndDate,
    IReadOnlyList<EventSalesTrendItem> Events);

public sealed record EventSalesTrendItem(
    Guid EventId,
    string Title,
    IReadOnlyList<EventSalesTrendPoint> SalesTrend);

public sealed record EventSalesTrendPoint(
    DateTime Time,
    int TicketsSold,
    decimal NetRevenue,
    decimal GrossRevenue);
