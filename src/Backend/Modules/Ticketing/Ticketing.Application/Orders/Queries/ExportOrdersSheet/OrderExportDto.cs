namespace Ticketing.Application.Orders.Queries.ExportOrdersSheet;

public sealed record OrderExportDto
{
    // Order
    public int Index { get; set; }
    public Guid OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;

    // Buyer
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;

    // Payment
    public decimal OriginalPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public string? CouponCode { get; set; }

    // Event
    public string EventName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime EventStartAt { get; set; }

    // Ticket
    public Guid? TicketId { get; set; }
    public string? TicketType { get; set; }
    public decimal? TicketPrice { get; set; }
    public string? TicketStatus { get; set; }
    public string? SessionTitle { get; set; }
    public DateTime? SessionStartTime { get; set; }
    public string? SeatCode { get; set; }
}
