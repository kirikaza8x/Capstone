using Microsoft.Extensions.Logging;
using Payment.Application.Features.VnPay.Dtos;
using Payment.Domain.Enums;
using Payments.Application.Features.Commands.WalletPay;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Payments.Commands.WalletPay;

public class WalletPayCommandHandler(
    IWalletRepository walletRepository,
    ICurrentUserService currentUser,
    IPaymentTransactionRepository transactionRepository,
    IPaymentUnitOfWork unitOfWork,
    ILogger<WalletPayCommandHandler> logger) : ICommandHandler<WalletPayCommand, WalletPayResultDto>
{
    public async Task<Result<WalletPayResultDto>> Handle(WalletPayCommand command, CancellationToken cancellationToken)
    {
        Guid userId = currentUser.UserId;
        // 1. Validate
        if (command.Amount <= 0)
            return Result.Failure<WalletPayResultDto>(
                Error.Validation("WalletPay.InvalidAmount", "Amount must be greater than zero."));

        // 2. Resolve wallet — must exist and be active for a payment
        var wallet = await walletRepository.GetByUserIdAsync(userId, cancellationToken);

        if (wallet == null)
            return Result.Failure<WalletPayResultDto>(
                Error.NotFound("WalletPay.WalletNotFound", "No wallet found. Please top up first."));

        if (wallet.Status != WalletStatus.Active)
            return Result.Failure<WalletPayResultDto>(
                Error.Failure("WalletPay.WalletNotActive",
                    $"Wallet is currently {wallet.Status} and cannot be used for payment."));

        // 3. Check balance before debit to give a clear error
        if (wallet.Balance < command.Amount)
            return Result.Failure<WalletPayResultDto>(
                Error.Failure("WalletPay.InsufficientFunds",
                    $"Wallet balance {wallet.Balance:N0} VND is insufficient for {command.Amount:N0} VND."));

        // 4. Debit wallet — creates a WalletTransaction internally
        WalletTransaction walletTxn;
        try
        {
            walletTxn = wallet.Debit(
                command.Amount,
                $"Event payment | EventId={command.EventId}" +
                (command.Description != null ? $" | {command.Description}" : string.Empty)
            );
        }
        catch (InvalidOperationException ex)
        {
            // Concurrent debit race guard
            logger.LogWarning(ex, "Debit race condition for UserId={UserId}", userId);
            return Result.Failure<WalletPayResultDto>(
                Error.Failure("WalletPay.DebitFailed", "Could not complete debit. Please try again."));
        }

        walletTxn.MarkCompleted();
        walletRepository.Update(wallet);

        // 5. Create a PaymentTransaction record for audit + refund traceability
        var paymentTxn = PaymentTransaction.CreateWalletPay(
            userId: userId,
            eventId: command.EventId,
            walletId: wallet.Id,
            amount: command.Amount,
            orderInfo: $"Wallet payment | EventId={command.EventId}"
        );

        transactionRepository.Add(paymentTxn);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "WalletPay completed: UserId={UserId}, EventId={EventId}, Amount={Amount}, BalanceAfter={Balance}",
            userId, command.EventId, command.Amount, wallet.Balance);

        return Result.Success(new WalletPayResultDto(
            PaymentTransactionId: paymentTxn.Id,
            WalletTransactionId: walletTxn.Id,
            AmountDebited: command.Amount,
            BalanceAfter: wallet.Balance,
            PaidAt: paymentTxn.CompletedAt!.Value
        ));
    }
}
