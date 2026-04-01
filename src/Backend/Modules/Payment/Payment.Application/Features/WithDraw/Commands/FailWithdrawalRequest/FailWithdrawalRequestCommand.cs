using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.WithdrawalRequests.Commands;

public record FailWithdrawalRequestCommand(
    Guid RequestId,
    string AdminNote) : ICommand;