using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Orders.Queries.GetEventReport;

public sealed record GetEventReportQuery(Guid EventId) : IQuery<EventReportResponse>;

public sealed record EventReportResponse(
    ReportSummary Summary,
    IReadOnlyCollection<ReportChartItem> Chart,
    IReadOnlyCollection<ReportTicketStat> TicketStats);

public sealed record ReportSummary(
    decimal Revenue,
    decimal RevenueTarget,
    int RevenueRate,
    int TicketsSold,
    int TotalTickets,
    int TicketsRate);

public sealed record ReportChartItem(
    string Time,
    decimal Revenue,
    int TicketsSold);

public sealed record ReportTicketStat(
    string TicketType,
    decimal Price,
    int Sold,
    int Total,
    int Locked,
    int Rate);
