namespace Ticketing.Application.Vouchers.Queries.Dto;

public sealed record VoucherDto
(
    Guid Id,
    string CouponCode,
    string Type,
    decimal Value,
    int TotalUse,
    int MaxUse,
    DateTime StartDate,
    DateTime EndDate,
    Guid? EventId,
    bool IsGlobal,
    DateTime? CreatedAt
);
