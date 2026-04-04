using AI.PublicApi.PublicApi;
using Microsoft.Extensions.Logging;
using Payment.Domain.Enums;
using Payments.Application.Abstractions;
using Payments.Application.Features.Payments.Commands.InitiatePayment;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payment.Application.Features.Vnpay.Commands.InitialPackagePayment;

public sealed class InitiatePackagePaymentCommandHandler(
    IVnPayService vnPayService,
    IAiPackagePublicApi aiPackagePublicApi,
    ICurrentUserService currentUserService,
    IPaymentTransactionRepository transactionRepository,
    IWalletRepository walletRepository,
    IPaymentUnitOfWork unitOfWork,
    ILogger<InitiatePackagePaymentCommandHandler> logger)
    : ICommandHandler<InitiatePackagePaymentCommand, InitiatePaymentResult>
{
    public async Task<Result<InitiatePaymentResult>> Handle(
        InitiatePackagePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
        {
            return Result.Failure<InitiatePaymentResult>(
                Error.Unauthorized("Payment.Unauthorized", "Current user is not authenticated."));
        }

        var ipAddress = currentUserService.IpAddress ?? "127.0.0.1";

        var package = await aiPackagePublicApi.GetPackageForPaymentAsync(
            command.PackageId,
            cancellationToken);

        if (package is null)
        {
            return Result.Failure<InitiatePaymentResult>(
                Error.NotFound("Payment.PackageNotFound", "AI package not found."));
        }

        if (!package.IsActive)
        {
            return Result.Failure<InitiatePaymentResult>(
                Error.Validation("Payment.PackageInactive", "AI package is inactive."));
        }

        var totalAmount = package.Price;
        var orderInfo = command.Description ?? $"Payment for AI package {command.PackageId}";

        return command.Method switch
        {
            PaymentType.BatchDirectPay => await HandleDirectPayAsync(
                userId,
                ipAddress,
                command.PackageId,
                totalAmount,
                orderInfo,
                cancellationToken),

            PaymentType.BatchWalletPay => await HandleWalletPayAsync(
                userId,
                command.PackageId,
                totalAmount,
                orderInfo,
                cancellationToken),

            _ => Result.Failure<InitiatePaymentResult>(
                Error.Validation("Payment.InvalidMethod", "Method must be BatchDirectPay or BatchWalletPay."))
        };
    }

    private async Task<Result<InitiatePaymentResult>> HandleDirectPayAsync(
        Guid userId,
        string ipAddress,
        Guid packageId,
        decimal totalAmount,
        string orderInfo,
        CancellationToken cancellationToken)
    {
        var txnRef = Guid.NewGuid().ToString("N");

        var txn = PaymentTransaction.CreateDirectPayByReference(
            userId: userId,
            referenceType: PaymentReferenceType.AiPackage,
            referenceId: packageId,
            amount: totalAmount,
            gatewayOrderInfo: orderInfo,
            gatewayTxnRef: txnRef,
            ipAddress: ipAddress);

        transactionRepository.Add(txn);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var url = vnPayService.CreatePaymentUrl(totalAmount, txnRef, orderInfo, ipAddress);

            return Result.Success(new InitiatePaymentResult(
                txn.Id,
                url,
                totalAmount,
                null));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "VNPay URL generation failed for package payment. TxnRef={TxnRef}", txnRef);

            txn.MarkFailed($"Gateway error: {ex.Message}");
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<InitiatePaymentResult>(
                Error.Failure("Payment.GatewayError", "Could not connect to payment provider."));
        }
    }

    private async Task<Result<InitiatePaymentResult>> HandleWalletPayAsync(
        Guid userId,
        Guid packageId,
        decimal totalAmount,
        string orderInfo,
        CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByUserIdAsync(userId, cancellationToken);

        if (wallet is null)
        {
            return Result.Failure<InitiatePaymentResult>(
                Error.NotFound("Payment.WalletNotFound", "No wallet found. Please top up first."));
        }

        if (wallet.Status == WalletStatus.Closed)
        {
            return Result.Failure<InitiatePaymentResult>(
                Error.Failure("Payment.WalletClosed", "Wallet is closed."));
        }

        if (wallet.Status == WalletStatus.Suspended)
        {
            return Result.Failure<InitiatePaymentResult>(
                Error.Failure("Payment.WalletSuspended", "Wallet is suspended. Please contact support."));
        }

        if (wallet.Balance < totalAmount)
        {
            return Result.Failure<InitiatePaymentResult>(
                Error.Failure("Payment.InsufficientFunds",
                    $"Balance {wallet.Balance:N0} VND is insufficient for {totalAmount:N0} VND."));
        }

        var walletTxn = wallet.Debit(
            totalAmount,
            $"AI package payment | PackageId={packageId} | {orderInfo}");

        walletTxn.MarkCompleted();
        walletRepository.Update(wallet);

        var txn = PaymentTransaction.CreateWalletPayByReference(
            userId: userId,
            walletId: wallet.Id,
            referenceType: PaymentReferenceType.AiPackage,
            referenceId: packageId,
            amount: totalAmount,
            orderInfo: orderInfo);

        transactionRepository.Add(txn);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new InitiatePaymentResult(
            txn.Id,
            null,
            totalAmount,
            txn.CompletedAt));
    }
}
