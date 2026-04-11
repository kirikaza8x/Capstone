using Payment.Domain.Enums;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Payments.Commands.InitiatePayment;

public record InitiatePaymentCommand(
    Guid OrderId,
    PaymentType Method,
    string? Description = null
) : ICommand<InitiatePaymentResult>;

public record InitiatePaymentResult(
    Guid PaymentTransactionId,
    string? PaymentUrl,
    decimal TotalAmount,
    DateTime? CompletedAt
);
