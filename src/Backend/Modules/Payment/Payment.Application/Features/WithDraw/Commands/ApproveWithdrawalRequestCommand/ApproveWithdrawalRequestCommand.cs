using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.WithdrawalRequests.Commands;

public sealed record ApproveWithdrawalRequestCommand(
    Guid RequestId,
    string? AdminNote
) : ICommand;