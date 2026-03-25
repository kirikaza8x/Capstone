namespace Ticketing.Application.Vouchers.Queries.Dto;

public sealed record VoucherDto
{
    public Guid Id { get; init; }
    public string CouponCode { get; init; }
    public string Type { get; init; }
    public decimal Value { get; init; }
    public int TotalUse { get; init; }
    public int MaxUse { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public Guid? EventId { get; init; }
    public bool IsGlobal { get; init; }
    public DateTime? CreatedAt { get; init; }
}
