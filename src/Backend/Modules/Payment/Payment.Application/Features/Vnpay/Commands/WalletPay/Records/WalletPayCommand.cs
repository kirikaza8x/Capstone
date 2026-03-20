using Payment.Application.Features.VnPay.Dtos;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Commands.WalletPay;

public record WalletPayCommand(
    // Guid UserId,
    Guid EventId,
    decimal Amount,
    string? Description = null
) : ICommand<WalletPayResultDto>;

