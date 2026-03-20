using Microsoft.Extensions.Logging;
using Payment.Application.Features.VnPay.Dtos;
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
    ILogger<VnPayReturnCommandHandler> logger) : ICommandHandler<VnPayReturnCommand, VnPayResultDto>
{
    public async Task<Result<VnPayResultDto>> Handle(VnPayReturnCommand command, CancellationToken cancellationToken)
    {
        // 1. Validate hash
        var vnPayResult = vnPayService.ValidateCallback(command.QueryParams);

        if (!vnPayResult.IsValid)
        {
            logger.LogWarning("VNPay callback hash validation failed.");
            return Result.Failure<VnPayResultDto>(Error.Failure("VnPay.InvalidSignature", "Security check failed."));
        }

        // 2. Resolve transaction
        if (string.IsNullOrEmpty(vnPayResult.OrderId))
            return Result.Failure<VnPayResultDto>(Error.Validation("VnPay.MissingTxnRef", "No transaction reference provided."));

        var transaction = await transactionRepository.GetByTxnRefAsync(vnPayResult.OrderId, cancellationToken);

        if (transaction == null)
        {
            logger.LogError("Transaction not found for TxnRef: {TxnRef}", vnPayResult.OrderId);
            return Result.Failure<VnPayResultDto>(Error.NotFound("Payment.NotFound", "Transaction reference not found."));
        }

        // 3. Always update gateway fields first — preserves real VNPay data regardless of outcome
        var gatewayStatus = command.QueryParams.TryGetValue("vnp_TransactionStatus", out var s) ? s : null;
        var bankCode = command.QueryParams.TryGetValue("vnp_BankCode", out var bc) ? bc : null;
        var bankTranNo = command.QueryParams.TryGetValue("vnp_BankTranNo", out var btn) ? btn : null;

        transaction.UpdateGatewayInfo(
            responseCode: vnPayResult.ResponseCode,
            status: gatewayStatus,
            transactionNo: vnPayResult.TransactionNo,
            bankCode: bankCode,
            bankTranNo: bankTranNo
        );

        // 4. Branch on outcome
        if (vnPayResult.IsSuccess)
        {
            transaction.MarkCompleted();

            if (transaction.Type == PaymentType.DirectPay)
            {
                logger.LogInformation("Direct pay completed for EventId: {EventId}", transaction.EventId);
                // TODO: publish domain event or send ticket generation command
            }
            else if (transaction.Type == PaymentType.WalletTopUp)
            {
                logger.LogInformation("Wallet top-up completed for WalletId: {WalletId}", transaction.WalletId);

                var wallet = await walletRepository.GetByUserIdAsync(transaction.UserId, cancellationToken);

                if (wallet == null)
                {
                    logger.LogError("Wallet not found for UserId: {UserId}", transaction.UserId);
                    return Result.Failure<VnPayResultDto>(Error.NotFound("Wallet.NotFound", "Wallet not found."));
                }

                var walletTxn = wallet.Credit(
                    transaction.Amount,
                    $"VNPay top-up ref {vnPayResult.TransactionNo}"
                );

                walletTxn.MarkCompleted(); // Don't leave WalletTransaction in Pending

                walletRepository.Update(wallet);
            }
        }
        else
        {
            transaction.MarkFailed(vnPayResult.Message);

            logger.LogWarning("Payment failed for TxnRef: {TxnRef}. Reason: {Reason}",
                vnPayResult.OrderId, vnPayResult.Message);
        }

        // 5. Persist
        transactionRepository.Update(transaction);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Return
        return Result.Success(new VnPayResultDto
        {
            ItemId = transaction.EventId ?? transaction.WalletId ?? Guid.Empty,
            PaymentSuccess = vnPayResult.IsSuccess,
            PaymentMessage = vnPayResult.Message,
            TransactionNo = vnPayResult.TransactionNo,
            ResponseCode = vnPayResult.ResponseCode,
            CheckedOutAt = transaction.CompletedAt ?? DateTime.UtcNow
        });
    }
}
