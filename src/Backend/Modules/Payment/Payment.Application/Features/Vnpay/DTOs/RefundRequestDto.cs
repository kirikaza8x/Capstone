using Payments.Domain.Enums;

namespace Payments.Application.DTOs.Refund;

public record RefundRequestDto(
    Guid Id,
    Guid UserId,
    Guid PaymentTransactionId,
    Guid? EventSessionId,
    RefundRequestScope Scope,
    RefundRequestStatus Status,
    decimal RequestedAmount,
    string UserReason,
    string? ReviewerNote,
    Guid? ReviewedByAdminId,
    DateTime? ReviewedAt,
    DateTime? CreatedAt
);
