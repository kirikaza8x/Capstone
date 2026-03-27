using Microsoft.Extensions.Logging;
using Payment.Domain.Enums;
using Payments.Application.Abstractions;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.PublicApi;

namespace Payments.Application.Features.Payments.Commands.InitiatePayment;

public class InitiatePaymentCommandHandler(
    IVnPayService vnPayService,
    ITicketingPublicApi ticketingApi,
    ICurrentUserService currentUserService,
    IPaymentTransactionRepository transactionRepository,
    IWalletRepository walletRepository,
    IPaymentUnitOfWork unitOfWork,
    ILogger<InitiatePaymentCommandHandler> logger)
    : ICommandHandler<InitiatePaymentCommand, InitiatePaymentResult>
{
    public async Task<Result<InitiatePaymentResult>> Handle(
        InitiatePaymentCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var ipAddress = currentUserService.IpAddress ?? "127.0.0.1";

        var order = await ticketingApi
            .GetOrderAsync(command.OrderId, userId, cancellationToken);

        if (order == null)
            return Result.Failure<InitiatePaymentResult>(
                Error.NotFound("Payment.OrderNotFound",
                    "Order not found or does not belong to you."));

        if (order.Tickets.Count == 0)
            return Result.Failure<InitiatePaymentResult>(
                Error.Validation("Payment.EmptyOrder",
                    "Order has no tickets."));

        if (order.Tickets.Any(t => t.Amount <= 0))
            return Result.Failure<InitiatePaymentResult>(
                Error.Validation("Payment.InvalidAmount",
                    "All ticket amounts must be greater than zero."));

        // Re-validate voucher MaxUse before charging — authoritative check at pay time
        var voucherValidation = await ticketingApi
            .ValidateOrderVoucherAsync(command.OrderId, cancellationToken);

        if (voucherValidation is { IsValid: false })
            return Result.Failure<InitiatePaymentResult>(
                Error.Validation("Payment.VoucherExceededMaxUse",
                    voucherValidation.ErrorMessage ?? "The voucher applied to this order has reached its maximum usage limit."));

        var totalAmount = order.Tickets.Sum(t => t.Amount);
        var orderInfo = command.Description
                          ?? $"Payment for order {command.OrderId}";
        var itemTuples = order.Tickets
            .Select(t => (t.OrderTicketId, t.EventSessionId, t.Amount));

        return command.Method switch
        {
            PaymentType.BatchDirectPay =>
                await HandleBatchDirectPayAsync(
                    command, userId, ipAddress,
                    totalAmount, orderInfo, itemTuples, cancellationToken),

            PaymentType.BatchWalletPay =>
                await HandleBatchWalletPayAsync(
                    command, userId,
                    totalAmount, orderInfo, itemTuples, cancellationToken),

            _ => Result.Failure<InitiatePaymentResult>(
                Error.Validation("Payment.InvalidMethod",
                    "Method must be BatchDirectPay or BatchWalletPay."))
        };
    }

    // -------------------------------------------------------------------------
    private async Task<Result<InitiatePaymentResult>> HandleBatchDirectPayAsync(
        InitiatePaymentCommand command,
        Guid userId,
        string ipAddress,
        decimal totalAmount,
        string orderInfo,
        IEnumerable<(Guid OrderTicketId, Guid EventSessionId, decimal Amount)> itemTuples,
        CancellationToken cancellationToken)
    {
        var txnRef = Guid.NewGuid().ToString("N");

        var txn = PaymentTransaction.CreateBatchDirectPay(
            userId, command.OrderId, itemTuples, orderInfo, txnRef, ipAddress);

        transactionRepository.Add(txn);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var url = vnPayService.CreatePaymentUrl(
                totalAmount, txnRef, orderInfo, ipAddress);

            logger.LogInformation(
                "BatchDirectPay initiated: UserId={UserId}, OrderId={OrderId}, " +
                "Total={Total}, TxnRef={TxnRef}",
                userId, command.OrderId, totalAmount, txnRef);

            return Result.Success(new InitiatePaymentResult(
                txn.Id, url, totalAmount, null));
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "VNPay URL generation failed TxnRef={TxnRef}", txnRef);

            txn.MarkFailed($"Gateway error: {ex.Message}");
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<InitiatePaymentResult>(
                Error.Failure("Payment.GatewayError",
                    "Could not connect to payment provider."));
        }
    }

    // -------------------------------------------------------------------------
    private async Task<Result<InitiatePaymentResult>> HandleBatchWalletPayAsync(
        InitiatePaymentCommand command,
        Guid userId,
        decimal totalAmount,
        string orderInfo,
        IEnumerable<(Guid OrderTicketId, Guid EventSessionId, decimal Amount)> itemTuples,
        CancellationToken cancellationToken)
    {
        var wallet = await walletRepository
            .GetByUserIdAsync(userId, cancellationToken);

        if (wallet == null)
            return Result.Failure<InitiatePaymentResult>(
                Error.NotFound("Payment.WalletNotFound",
                    "No wallet found. Please top up first."));

        if (wallet.Status == WalletStatus.Closed)
            return Result.Failure<InitiatePaymentResult>(
                Error.Failure("Payment.WalletClosed",
                    "Wallet is closed."));

        if (wallet.Status == WalletStatus.Suspended)
            return Result.Failure<InitiatePaymentResult>(
                Error.Failure("Payment.WalletSuspended",
                    "Wallet is suspended. Please contact support."));

        if (wallet.Balance < totalAmount)
            return Result.Failure<InitiatePaymentResult>(
                Error.Failure("Payment.InsufficientFunds",
                    $"Balance {wallet.Balance:N0} VND is insufficient " +
                    $"for {totalAmount:N0} VND."));

        WalletTransaction walletTxn;
        try
        {
            walletTxn = wallet.Debit(
                totalAmount,
                $"Order payment | OrderId={command.OrderId} | {orderInfo}");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex,
                "Debit race for UserId={UserId}", userId);
            return Result.Failure<InitiatePaymentResult>(
                Error.Failure("Payment.DebitFailed",
                    "Debit failed. Please try again."));
        }

        walletTxn.MarkCompleted();
        walletRepository.Update(wallet);

        var txn = PaymentTransaction.CreateBatchWalletPay(
            userId, wallet.Id, command.OrderId, itemTuples, orderInfo);

        transactionRepository.Add(txn);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "BatchWalletPay completed: UserId={UserId}, OrderId={OrderId}, " +
            "Total={Total}, BalanceAfter={Balance}",
            userId, command.OrderId, totalAmount, wallet.Balance);

        return Result.Success(new InitiatePaymentResult(
            txn.Id, null, totalAmount, txn.CompletedAt));
    }
}
