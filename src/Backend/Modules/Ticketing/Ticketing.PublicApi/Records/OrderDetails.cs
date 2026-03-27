namespace Ticketing.PublicApi.Records;

public record OrderDetails(
    Guid OrderId,
    Guid UserId,
    decimal TotalAmount,
    IReadOnlyList<OrderTicketDetail> Tickets
);

public record OrderTicketDetail(
    Guid OrderTicketId,
    Guid EventSessionId,
    decimal Amount
);

/// <summary>IsValid=false means the voucher's MaxUse has been exceeded.</summary>
public record VoucherValidationResult(bool IsValid, string? ErrorMessage = null);

