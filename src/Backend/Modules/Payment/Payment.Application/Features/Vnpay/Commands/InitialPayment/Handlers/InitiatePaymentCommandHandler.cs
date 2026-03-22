using Microsoft.Extensions.Logging;
using Payment.Domain.Enums;
using Payments.Application.Abstractions;
using Payments.Application.DTOs.Payment;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Payments.Commands.InitiatePayment;

public class InitiatePaymentCommandHandler(
    ICurrentUserService currentUser,
    IVnPayService vnPayService,
    IPaymentTransactionRepository transactionRepository,
    IWalletRepository walletRepository,
    IPaymentUnitOfWork unitOfWork,
    ILogger<InitiatePaymentCommandHandler> logger)
    : ICommandHandler<InitiatePaymentCommand, InitiatePaymentResult>
{
    public async Task<Result<InitiatePaymentResult>> Handle(
        InitiatePaymentCommand command, CancellationToken cancellationToken)
    {
        // --- Common validation ---
        if (command.Items.Count == 0)
            return Result.Failure<InitiatePaymentResult>(
                Error.Validation("Payment.Empty", "At least one item is required."));

        if (command.Items.Any(i => i.Amount <= 0))
            return Result.Failure<InitiatePaymentResult>(
                Error.Validation("Payment.InvalidAmount", "All item amounts must be greater than zero."));

        if (command.Items.Select(i => i.EventId).Distinct().Count() != command.Items.Count)
            return Result.Failure<InitiatePaymentResult>(
                Error.Validation("Payment.DuplicateEvent", "Duplicate events in the same payment are not allowed."));

        var totalAmount = command.Items.Sum(i => i.Amount);
        var orderInfo = command.Description ?? $"Payment for {command.Items.Count} event(s)";
        var itemTuples = command.Items.Select(i => (i.EventId, i.Amount));

        return command.Method switch
        {
            PaymentType.BatchDirectPay =>
                await HandleBatchDirectPayAsync(
                    command, totalAmount, orderInfo, itemTuples, cancellationToken),

            PaymentType.BatchWalletPay =>
                await HandleBatchWalletPayAsync(
                    command, totalAmount, orderInfo, itemTuples, cancellationToken),

            _ => Result.Failure<InitiatePaymentResult>(
                Error.Validation("Payment.InvalidMethod",
                    "Method must be BatchDirectPay or BatchWalletPay."))
        };
    }

    // -------------------------------------------------------------------------
    private async Task<Result<InitiatePaymentResult>> HandleBatchDirectPayAsync(
        InitiatePaymentCommand command,
        decimal totalAmount,
        string orderInfo,
        IEnumerable<(Guid EventId, decimal Amount)> itemTuples,
        CancellationToken cancellationToken)
    {
        var txnRef = Guid.NewGuid().ToString("N");

        var txn = PaymentTransaction.CreateBatchDirectPay(
            currentUser.UserId, itemTuples, orderInfo, txnRef);

        transactionRepository.Add(txn);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var url = vnPayService.CreatePaymentUrl(
                totalAmount, txnRef, orderInfo,currentUser.IpAddress);

            logger.LogInformation(
                "BatchDirectPay initiated: UserId={UserId}, Items={Count}, Total={Total}, TxnRef={TxnRef}",
                currentUser.UserId, command.Items.Count, totalAmount, txnRef);

            return Result.Success(new InitiatePaymentResult(
                txn.Id, url, totalAmount, null));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "VNPay URL generation failed TxnRef={TxnRef}", txnRef);
            txn.MarkFailed($"Gateway error: {ex.Message}");
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<InitiatePaymentResult>(
                Error.Failure("Payment.GatewayError", "Could not connect to payment provider."));
        }
    }

    // -------------------------------------------------------------------------
    private async Task<Result<InitiatePaymentResult>> HandleBatchWalletPayAsync(
        InitiatePaymentCommand command,
        decimal totalAmount,
        string orderInfo,
        IEnumerable<(Guid EventId, decimal Amount)> itemTuples,
        CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByUserIdAsync(currentUser.UserId, cancellationToken);

        if (wallet == null)
            return Result.Failure<InitiatePaymentResult>(
                Error.NotFound("Payment.WalletNotFound",
                    "No wallet found. Please top up first."));

        if (wallet.Status == WalletStatus.Closed)
            return Result.Failure<InitiatePaymentResult>(
                Error.Failure("Payment.WalletClosed", "Wallet is closed."));

        if (wallet.Status == WalletStatus.Suspended)
            return Result.Failure<InitiatePaymentResult>(
                Error.Failure("Payment.WalletSuspended",
                    "Wallet is suspended. Please contact support."));

        if (wallet.Balance < totalAmount)
            return Result.Failure<InitiatePaymentResult>(
                Error.Failure("Payment.InsufficientFunds",
                    $"Balance {wallet.Balance:N0} VND is insufficient for {totalAmount:N0} VND."));

        WalletTransaction walletTxn;
        try
        {
            walletTxn = wallet.Debit(totalAmount,
                $"Batch wallet payment | {command.Items.Count} event(s) | {orderInfo}");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Debit race for UserId={UserId}", currentUser.UserId);
            return Result.Failure<InitiatePaymentResult>(
                Error.Failure("Payment.DebitFailed", "Debit failed. Please try again."));
        }

        walletTxn.MarkCompleted();
        walletRepository.Update(wallet);

        var txn = PaymentTransaction.CreateBatchWalletPay(
            currentUser.UserId, wallet.Id, itemTuples, orderInfo);

        transactionRepository.Add(txn);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "BatchWalletPay completed: UserId={UserId}, Items={Count}, Total={Total}, BalanceAfter={Balance}",
            currentUser.UserId, command.Items.Count, totalAmount, wallet.Balance);

        return Result.Success(new InitiatePaymentResult(
            txn.Id, null, totalAmount, txn.CompletedAt));
    }
}