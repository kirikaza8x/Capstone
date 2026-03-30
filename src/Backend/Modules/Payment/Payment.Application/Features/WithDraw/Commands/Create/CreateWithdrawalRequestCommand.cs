using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.WithdrawalRequests.Commands;

public sealed record CreateWithdrawalRequestCommand(
    string BankAccountNumber,
    string BankName,
    decimal Amount,
    string? Notes
) : ICommand<Guid>;