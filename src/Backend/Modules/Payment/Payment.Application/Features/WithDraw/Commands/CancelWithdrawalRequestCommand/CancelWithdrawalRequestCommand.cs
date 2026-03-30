using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.WithdrawalRequests.Commands;

public sealed record CancelWithdrawalRequestCommand(
    Guid RequestId
) : ICommand;