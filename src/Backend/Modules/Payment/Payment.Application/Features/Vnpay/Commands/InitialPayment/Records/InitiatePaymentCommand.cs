using Payment.Domain.Enums;
using Payments.Application.DTOs.Payment;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Payments.Commands.InitiatePayment;

public record InitiatePaymentCommand(
    PaymentType Method,                         // BatchDirectPay or BatchWalletPay
    IReadOnlyList<PaymentItemDto> Items,
    string? Description = null
) : ICommand<InitiatePaymentResult>;

public record InitiatePaymentResult(
    Guid PaymentTransactionId,
    string? PaymentUrl,                         // null for BatchWalletPay
    decimal TotalAmount,
    DateTime? CompletedAt                       // populated immediately for BatchWalletPay
);