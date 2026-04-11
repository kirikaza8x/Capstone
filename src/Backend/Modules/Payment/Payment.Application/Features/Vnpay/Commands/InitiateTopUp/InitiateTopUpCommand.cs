using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Payments.Commands.InitiateTopUp;

public record InitiateTopUpCommand(
    decimal Amount,
    string? Description = null
) : ICommand<InitiateTopUpResult>;

public record InitiateTopUpResult(
    Guid PaymentTransactionId,
    string PaymentUrl
);
