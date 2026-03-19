using Payment.Application.Features.VnPay.Dtos;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Commands.InitiatePayment;

public record InitiatePaymentCommand(
    string OrderId,
    decimal Amount,
    string IpAddress
) : ICommand<InitiatePaymentResponseDto>;