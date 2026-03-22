using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Payments.Commands.GetPaymentUrl;

public record GetPaymentUrlCommand(
    Guid PaymentTransactionId
) : ICommand<GetPaymentUrlResult>;

public record GetPaymentUrlResult(
    Guid PaymentTransactionId,
    string PaymentUrl,
    decimal Amount,
    Payment.Domain.Enums.PaymentInternalStatus InternalStatus
);