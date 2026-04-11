namespace Ticketing.Application.Orders.Queries.ExportOrdersSheet;

public sealed record OrderExportDto
{
    public int Index { get; set; }
    public Guid OrderId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string? CouponCode { get; set; }
    public string? VoucherType { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
