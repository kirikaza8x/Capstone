namespace Ticketing.Application.Orders.Queries.ExportVoucherSheet;

public sealed record VoucherExportDto
{
    public int Index { get; set; }
    public Guid Id { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Used { get; set; } = string.Empty; 
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string IsActive { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
