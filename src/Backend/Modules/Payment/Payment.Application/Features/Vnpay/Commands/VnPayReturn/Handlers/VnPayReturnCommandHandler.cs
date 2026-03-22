using Microsoft.Extensions.Logging;
using Payment.Domain.Enums;
using Payments.Application.Abstractions;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Payments.Commands.VnPayReturn;

public class VnPayReturnCommandHandler(
    IVnPayService vnPayService,
    IPaymentTransactionRepository transactionRepository,
    IWalletRepository walletRepository,
    IPaymentUnitOfWork unitOfWork,
    ILogger<VnPayReturnCommandHandler> logger)
    : ICommandHandler<VnPayReturnCommand, VnPayReturnResult>
{
    public async Task<Result<VnPayReturnResult>> Handle(
        VnPayReturnCommand command, CancellationToken cancellationToken)
    {
        // 1. Validate hash
        var callback = vnPayService.ValidateCallback(command.QueryParams);

        if (!callback.IsValid)
        {
            logger.LogWarning("VNPay callback hash validation failed.");
            return Result.Failure<VnPayReturnResult>(
                Error.Failure("VnPay.InvalidSignature", "Security check failed."));
        }

        if (string.IsNullOrEmpty(callback.OrderId))
            return Result.Failure<VnPayReturnResult>(
                Error.Validation("VnPay.MissingTxnRef", "No transaction reference provided."));

        // 2. Load transaction with items — always with items
        var txn = await transactionRepository
            .GetByTxnRefWithItemsAsync(callback.OrderId, cancellationToken);

        if (txn == null)
        {
            logger.LogError("Transaction not found for TxnRef={TxnRef}", callback.OrderId);
            return Result.Failure<VnPayReturnResult>(
                Error.NotFound("Payment.NotFound", "Transaction not found."));
        }

        // 3. Always update gateway fields first — preserves real VNPay data
        command.QueryParams.TryGetValue("vnp_TransactionStatus", out var gatewayStatus);
        command.QueryParams.TryGetValue("vnp_BankCode", out var bankCode);
        command.QueryParams.TryGetValue("vnp_BankTranNo", out var bankTranNo);

        txn.UpdateGatewayInfo(
            responseCode: callback.ResponseCode,
            status: gatewayStatus,
            transactionNo: callback.TransactionNo,
            bankCode: bankCode,
            bankTranNo: bankTranNo
        );

        // 4. Branch on outcome
        if (callback.IsSuccess)
        {
            txn.MarkCompleted(); // propagates to all BatchPaymentItems automatically

            if (txn.Type == PaymentType.WalletTopUp)
            {
                var wallet = await walletRepository
                    .GetByUserIdAsync(txn.UserId, cancellationToken);

                if (wallet == null)
                    return Result.Failure<VnPayReturnResult>(
                        Error.NotFound("Wallet.NotFound", "Wallet not found."));

                var walletTxn = wallet.Credit(
                    txn.Amount,
                    $"VNPay top-up | TxnNo={callback.TransactionNo}");

                walletTxn.MarkCompleted();
                walletRepository.Update(wallet);

                logger.LogInformation(
                    "WalletTopUp completed: UserId={UserId}, Amount={Amount}",
                    txn.UserId, txn.Amount);
            }
            else
            {
                // BatchDirectPay — items already marked via MarkCompleted()
                logger.LogInformation(
                    "BatchDirectPay completed: TxnRef={TxnRef}, Items={Count}",
                    callback.OrderId, txn.Items.Count);
            }
        }
        else
        {
            txn.MarkFailed(callback.Message);

            logger.LogWarning(
                "Payment failed: TxnRef={TxnRef}, Code={Code}, Reason={Reason}",
                callback.OrderId, callback.ResponseCode, callback.Message);
        }

        // 5. Persist
        transactionRepository.Update(txn);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new VnPayReturnResult(
            PaymentTransactionId: txn.Id,
            IsSuccess: callback.IsSuccess,
            Message: callback.Message,
            ResponseCode: callback.ResponseCode,
            TransactionNo: callback.TransactionNo,
            Type: txn.Type,
            CompletedAt: txn.CompletedAt
        ));
    }
}