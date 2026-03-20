using Microsoft.Extensions.Logging;
using Payment.Application.Features.VnPay.Dtos;
using Payments.Application.Features.Commands.RefundByEvent;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Payments.Commands.RefundByEvent;

public class RefundByEventCommandHandler(
    IPaymentTransactionRepository transactionRepository,
    IWalletRepository walletRepository,
    IPaymentUnitOfWork unitOfWork,
    ILogger<RefundByEventCommandHandler> logger) : ICommandHandler<RefundByEventCommand, RefundByEventResultDto>
{
    public async Task<Result<RefundByEventResultDto>> Handle(RefundByEventCommand command, CancellationToken cancellationToken)
    {
        // 1. Find completed DirectPay or WalletPay for this user + event
        var transaction = await transactionRepository.GetCompletedByEventIdAsync(
            command.EventId, command.UserId, cancellationToken);

        if (transaction == null)
            return Result.Failure<RefundByEventResultDto>(
                Error.NotFound("Refund.NotFound",
                    "No completed payment found for this event."));

        if (transaction.InternalStatus == Payment.Domain.Enums.PaymentInternalStatus.Refunded)
            return Result.Failure<RefundByEventResultDto>(
                Error.Conflict("Refund.AlreadyRefunded",
                    "This payment has already been refunded."));

        // 2. Resolve or create wallet for refund recipient
        var wallet = await ResolveWalletForRefundAsync(command.UserId, cancellationToken);

        // 3. Credit + complete
        var walletTxn = wallet.Credit(
            transaction.Amount,
            $"Refund | EventId={command.EventId} | TxnRef={transaction.GatewayTxnRef ?? transaction.Id.ToString()}"
        );
        walletTxn.MarkCompleted();

        // 4. Mark payment transaction refunded
        transaction.MarkRefunded();

        transactionRepository.Update(transaction);
        walletRepository.Update(wallet);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Single refund issued: EventId={EventId}, UserId={UserId}, Amount={Amount}",
            command.EventId, command.UserId, transaction.Amount);

        return Result.Success(new RefundByEventResultDto(
            PaymentTransactionId: transaction.Id,
            AmountRefunded: transaction.Amount,
            WalletBalanceAfter: wallet.Balance,
            RefundedAt: transaction.RefundedAt!.Value
        ));
    }

    private async Task<Wallet> ResolveWalletForRefundAsync(Guid userId, CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByUserIdAsync(userId, cancellationToken);

        if (wallet == null)
        {
            wallet = Wallet.Create(userId, initialBalance: 0);
            walletRepository.Add(wallet);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return wallet;
        }

        if (wallet.Status == WalletStatus.Suspended)
        {
            wallet.ChangeStatus(WalletStatus.Active);
            walletRepository.Update(wallet);
        }

        return wallet;
    }
}
