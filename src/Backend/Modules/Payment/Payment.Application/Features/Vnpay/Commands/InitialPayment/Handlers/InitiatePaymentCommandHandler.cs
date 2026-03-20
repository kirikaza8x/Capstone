using Microsoft.Extensions.Logging;
using Payment.Application.Features.VnPay.Dtos;
using Payment.Domain.Enums;
using Payments.Application.Abstractions;
using Payments.Application.Features.Commands.InitiatePayment;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Payments.Commands.InitiatePayment;

public class InitiatePaymentCommandHandler(
    IVnPayService vnPayService,
    IPaymentTransactionRepository transactionRepository,
    IWalletRepository walletRepository,
    IPaymentUnitOfWork unitOfWork,
    ILogger<InitiatePaymentCommandHandler> logger) : ICommandHandler<InitiatePaymentCommand, InitiatePaymentResponseDto>
{
    public async Task<Result<InitiatePaymentResponseDto>> Handle(InitiatePaymentCommand command, CancellationToken cancellationToken)
    {
        // 1. Basic validation
        if (command.Amount <= 0)
            return Result.Failure<InitiatePaymentResponseDto>(
                Error.Validation("Payment.InvalidAmount", "Amount must be greater than zero."));

        string orderDescription = command.Description ?? $"Payment for {command.Type}";
        string internalTxnRef = Guid.NewGuid().ToString("N");

        PaymentTransaction transaction;

        if (command.Type == PaymentType.DirectPay)
        {
            // 2a. DirectPay — EventId is required
            if (!command.EventId.HasValue)
                return Result.Failure<InitiatePaymentResponseDto>(
                    Error.Validation("Payment.MissingEventId", "EventId is required for Direct Ticket Purchase."));

            transaction = PaymentTransaction.CreateDirectPay(
                command.UserId,
                command.EventId.Value,
                command.Amount,
                orderDescription,
                internalTxnRef
            );
        }
        else
        {
            var wallet = await ResolveOrCreateWalletAsync(command.UserId, cancellationToken);

            transaction = PaymentTransaction.CreateWalletTopUp(
                command.UserId,
                wallet.Id,
                command.Amount,
                orderDescription,
                internalTxnRef
            );
        }

        transactionRepository.Add(transaction);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var paymentUrl = vnPayService.CreatePaymentUrl(
                transaction.Amount,
                transaction.GatewayTxnRef!,
                transaction.GatewayOrderInfo ?? "Order Payment",
                command.IpAddress
            );

            return Result.Success(new InitiatePaymentResponseDto(paymentUrl));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "VNPay URL generation failed for TxnRef: {TxnRef}", internalTxnRef);
            transaction.MarkFailed($"Gateway error: {ex.Message}");
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<InitiatePaymentResponseDto>(
                Error.Failure("Payment.GatewayError", "Could not connect to the payment provider."));
        }
    }

    // Finds the user's active wallet or creates one on the spot
    private async Task<Wallet> ResolveOrCreateWalletAsync(Guid userId, CancellationToken cancellationToken)
    {
        var existing = await walletRepository.GetByUserIdAsync(userId, cancellationToken);

        if (existing != null)
        {
            if (existing.Status == WalletStatus.Suspended)
            {
                logger.LogInformation("Reactivating suspended wallet {WalletId} for user {UserId}",
                    existing.Id, userId);
                existing.ChangeStatus(WalletStatus.Active);
                walletRepository.Update(existing);
            }

            return existing;
        }

        logger.LogInformation("No wallet found for user {UserId} — creating one", userId);

        var wallet = Wallet.Create(userId, initialBalance: 0);
        walletRepository.Add(wallet);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return wallet;
    }
}
