using Payment.Application.Features.VnPay.Dtos;
using Payment.Domain.Enums;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Commands.InitiatePayment;

public record InitiatePaymentCommand(
    Guid UserId,
    string IpAddress,
    decimal Amount,
    PaymentType Type,
    Guid? EventId,          // required only for DirectPay
    string? Description = null
) : ICommand<InitiatePaymentResponseDto>;
