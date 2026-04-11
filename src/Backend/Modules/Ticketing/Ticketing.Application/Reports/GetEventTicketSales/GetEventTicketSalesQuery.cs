using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Reports.GetEventTicketSales;

public sealed record GetEventTicketSalesQuery(Guid EventId) : IQuery<TicketSalesResponse>;

public sealed record TicketSalesResponse(
    Guid EventId,
    TicketSalesSummaryDto Summary,
    IReadOnlyList<TicketTypeSalesDto> TicketTypeBreakdown);

public sealed record TicketSalesSummaryDto(
    int TotalOrders,
    int TotalTicketsSold,
    int TotalTicketsCheckedIn,
    double CheckInRate,
    decimal GrossRevenue,
    decimal TotalDiscount,
    decimal NetRevenue);

public sealed record TicketTypeSalesDto(
    Guid TicketTypeId,
    string TicketTypeName,
    int TotalQuantity,
    int QuantitySold,
    int QuantityCheckedIn,
    decimal Revenue);
