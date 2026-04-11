namespace Payment.Application.Features.VnPay.Dtos;

public record RefundByEventResultDto(
    Guid PaymentTransactionId,
    decimal AmountRefunded,
    decimal WalletBalanceAfter,
    DateTime RefundedAt
);

public record MassRefundItemResult(
    Guid PaymentTransactionId,
    Guid UserId,
    decimal Amount,
    bool Success,
    string? FailureReason
);

public record MassRefundResultDto(
    Guid EventId,
    int TotalFound,
    int Succeeded,
    int Skipped,       // already refunded
    int Failed,        // unexpected errors
    IReadOnlyList<MassRefundItemResult> Items
);
