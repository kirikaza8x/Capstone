using Payment.Domain.Enums;

namespace Payments.Application.DTOs.Payment;

public record PaymentTransactionDto(
    Guid Id,
    PaymentType Type,
    PaymentInternalStatus InternalStatus,
    decimal Amount,
    string Currency,
    Guid? OrderId,
    IReadOnlyList<BatchPaymentItemDto> Items,
    string? GatewayTxnRef,
    string? GatewayTransactionNo,
    string? GatewayResponseCode,
    string? GatewayBankCode,
    DateTime? CreatedAt,
    DateTime? CompletedAt,
    DateTime? FailedAt,
    DateTime? RefundedAt
);

public record BatchPaymentItemDto(
    Guid Id,
    Guid OrderTicketId,
    Guid EventSessionId,
    decimal Amount,
    PaymentInternalStatus InternalStatus,
    DateTime? RefundedAt,
    DateTime? CreatedAt
);
