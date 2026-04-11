using Microsoft.Extensions.Logging;
using Payment.Domain.Enums;
using Payments.Application.Abstractions;
using Payments.Application.DTOs.VnPay;
using Payments.Application.Features.Payments.Commands.VnPayReturn;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payment.Application.Features.Vnpay.Commands.VnPayReturn.Handlers;

public class VnPayReturnCommandHandler(
    IVnPayService vnPayService,
    IPaymentTransactionRepository transactionRepository,
    IWalletRepository walletRepository,
    IPaymentUnitOfWork unitOfWork,
    ILogger<VnPayReturnCommandHandler> logger)
    : ICommandHandler<VnPayReturnCommand, VnPayReturnResult>
{
    public async Task<Result<VnPayReturnResult>> Handle(
        VnPayReturnCommand command,
        CancellationToken cancellationToken)
    {
        var callback = vnPayService.ValidateCallback(command.QueryParams);

        logger.LogInformation(
            "VNPayReturnCommand received with QueryParams: {@QueryParams}",
            command.QueryParams);

        var validationResult = ValidateCallback(callback);
        if (validationResult.IsFailure)
        {
            return Result.Failure<VnPayReturnResult>(validationResult.Error);
        }

        var txn = await transactionRepository.GetByTxnRefWithItemsAsync(
            callback.OrderId!,
            cancellationToken);

        if (txn is null)
        {
            logger.LogError("Transaction not found for TxnRef={TxnRef}", callback.OrderId);
            return Result.Failure<VnPayReturnResult>(
                Error.NotFound("Payment.NotFound", "Transaction not found."));
        }

        UpdateGatewayInfo(txn, callback, command.QueryParams);

        var wasCompletedOrRefunded = txn.InternalStatus is
            PaymentInternalStatus.Completed or PaymentInternalStatus.Refunded;

        var processingResult = callback.IsSuccess
            ? await HandleSuccessAsync(txn, callback, wasCompletedOrRefunded, cancellationToken)
            : HandleFailure(txn, callback, wasCompletedOrRefunded);

        transactionRepository.Update(txn);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (processingResult.IsFailure)
        {
            return Result.Failure<VnPayReturnResult>(processingResult.Error);
        }

        return Result.Success(new VnPayReturnResult(
            PaymentTransactionId: txn.Id,
            IsSuccess: callback.IsSuccess,
            Message: callback.Message,
            ResponseCode: callback.ResponseCode,
            TransactionNo: callback.TransactionNo,
            Type: txn.Type,
            CompletedAt: txn.CompletedAt));
    }

    private static Result ValidateCallback(PaymentCallbackResult callback)
    {
        if (!callback.IsValid)
        {
            return Result.Failure(Error.Failure(
                "VnPay.InvalidSignature",
                "Security check failed."));
        }

        if (string.IsNullOrEmpty(callback.OrderId))
        {
            return Result.Failure(Error.Validation(
                "VnPay.MissingTxnRef",
                "No transaction reference provided."));
        }

        return Result.Success();
    }

    private static void UpdateGatewayInfo(
        Payments.Domain.Entities.PaymentTransaction txn,
        PaymentCallbackResult callback,
        IDictionary<string, string> queryParams)
    {
        queryParams.TryGetValue("vnp_TransactionStatus", out var gatewayStatus);
        queryParams.TryGetValue("vnp_BankCode", out var bankCode);
        queryParams.TryGetValue("vnp_BankTranNo", out var bankTranNo);
        queryParams.TryGetValue("vnp_CardType", out var cardType);
        queryParams.TryGetValue("vnp_PayDate", out var payDate);
        queryParams.TryGetValue("vnp_TmnCode", out var tmnCode);
        queryParams.TryGetValue("vnp_SecureHash", out var secureHash);
        queryParams.TryGetValue("vnp_SecureHashType", out var secureHashType);
        queryParams.TryGetValue("vnp_Locale", out var locale);
        queryParams.TryGetValue("vnp_OrderInfo", out var orderInfo);

        txn.UpdateGatewayInfo(
            responseCode: callback.ResponseCode,
            status: gatewayStatus,
            transactionNo: callback.TransactionNo,
            bankCode: bankCode,
            bankTranNo: bankTranNo,
            cardType: cardType,
            payDate: payDate,
            tmnCode: tmnCode,
            secureHash: secureHash,
            orderInfo: orderInfo,
            secureHashType: secureHashType,
            locale: locale);
    }

    private async Task<Result> HandleSuccessAsync(
        Payments.Domain.Entities.PaymentTransaction txn,
        PaymentCallbackResult callback,
        bool wasCompletedOrRefunded,
        CancellationToken cancellationToken)
    {
        txn.MarkCompleted();

        if (txn.Type != PaymentType.WalletTopUp || wasCompletedOrRefunded)
        {
            return Result.Success();
        }

        var wallet = await walletRepository.GetByUserIdAsync(txn.UserId, cancellationToken);
        if (wallet is null)
        {
            logger.LogError(
                "Wallet not found for successful WalletTopUp. UserId={UserId}, TxnId={TxnId}",
                txn.UserId,
                txn.Id);

            return Result.Failure(Error.NotFound("Wallet.NotFound", "Wallet not found."));
        }

        var walletTxn = wallet.Credit(
            txn.Amount,
            $"VNPay top-up | TxnNo={callback.TransactionNo}");

        walletTxn.MarkCompleted();
        walletRepository.Update(wallet);

        logger.LogInformation(
            "WalletTopUp completed: UserId={UserId}, Amount={Amount}",
            txn.UserId,
            txn.Amount);

        return Result.Success();
    }

    private Result HandleFailure(
        Payments.Domain.Entities.PaymentTransaction txn,
        PaymentCallbackResult callback,
        bool wasCompletedOrRefunded)
    {
        if (!wasCompletedOrRefunded)
        {
            txn.MarkFailed(callback.Message);
        }

        logger.LogWarning(
            "Payment failed callback: TxnRef={TxnRef}, Code={Code}, Reason={Reason}",
            callback.OrderId,
            callback.ResponseCode,
            callback.Message);

        return Result.Success();
    }
}
