using Payment.Domain.Enums;

namespace Payments.Application.DTOs.Payment;

public record PaymentTransactionDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string? Username { get; set; }
    public PaymentType Type { get; init; }
    public PaymentReferenceType? ReferenceType { get; init; }
    public Guid? ReferenceId { get; init; }

    public PaymentInternalStatus InternalStatus { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "VND";
    public Guid? OrderId { get; init; }
    public string? GatewayTxnRef { get; init; }
    public string? GatewayTransactionNo { get; init; }
    public string? GatewayResponseCode { get; init; }
    public string? GatewayBankCode { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime? FailedAt { get; init; }
    public DateTime? RefundedAt { get; init; }
}

public record PaymentTransactionDetailDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string? Username { get; set; }
    public PaymentType Type { get; init; }
    public PaymentReferenceType? ReferenceType { get; init; }
    public Guid? ReferenceId { get; init; }

    public PaymentInternalStatus InternalStatus { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "VND";
    public Guid? OrderId { get; init; }
    public IReadOnlyList<BatchPaymentItemDto> Items { get; init; } = new List<BatchPaymentItemDto>();
    public string? GatewayTxnRef { get; init; }
    public string? GatewayTransactionNo { get; init; }
    public string? GatewayResponseCode { get; init; }
    public string? GatewayBankCode { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime? FailedAt { get; init; }
    public DateTime? RefundedAt { get; init; }
}

public record BatchPaymentItemDto(
    Guid Id,
    Guid OrderTicketId,
    Guid EventSessionId,
    decimal Amount,
    PaymentInternalStatus InternalStatus,
    DateTime? RefundedAt,
    DateTime? CreatedAt
);
