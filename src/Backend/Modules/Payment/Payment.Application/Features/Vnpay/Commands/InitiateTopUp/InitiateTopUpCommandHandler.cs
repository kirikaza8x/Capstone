using Microsoft.Extensions.Logging;
using Payments.Application.Abstractions;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Payments.Commands.InitiateTopUp;

public class InitiateTopUpCommandHandler(
    IVnPayService vnPayService,
    IPaymentTransactionRepository transactionRepository,
    IWalletRepository walletRepository,
    IPaymentUnitOfWork unitOfWork,
    ILogger<InitiateTopUpCommandHandler> logger)
    : ICommandHandler<InitiateTopUpCommand, InitiateTopUpResult>
{
    public async Task<Result<InitiateTopUpResult>> Handle(
        InitiateTopUpCommand command, CancellationToken cancellationToken)
    {
        if (command.Amount <= 0)
            return Result.Failure<InitiateTopUpResult>(
                Error.Validation("TopUp.InvalidAmount", "Amount must be greater than zero."));

        // Resolve or create wallet
        var wallet = await walletRepository.GetByUserIdAsync(command.UserId, cancellationToken);

        if (wallet == null)
        {
            wallet = Wallet.Create(command.UserId);
            walletRepository.Add(wallet);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Wallet auto-created for UserId={UserId}", command.UserId);
        }
        else if (wallet.Status == WalletStatus.Closed)
        {
            return Result.Failure<InitiateTopUpResult>(
                Error.Failure("TopUp.WalletClosed", "Cannot top up a closed wallet."));
        }
        else if (wallet.Status == WalletStatus.Suspended)
        {
            wallet.ChangeStatus(WalletStatus.Active);
            walletRepository.Update(wallet);
        }

        var txnRef = Guid.NewGuid().ToString("N");
        var orderInfo = command.Description ?? $"Wallet top-up {command.Amount:N0} VND";

        var txn = PaymentTransaction.CreateWalletTopUp(
            command.UserId, wallet.Id, command.Amount, orderInfo, txnRef);

        transactionRepository.Add(txn);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var url = vnPayService.CreatePaymentUrl(
                command.Amount, txnRef, orderInfo, command.IpAddress);

            logger.LogInformation(
                "TopUp initiated: UserId={UserId}, Amount={Amount}, TxnRef={TxnRef}",
                command.UserId, command.Amount, txnRef);

            return Result.Success(new InitiateTopUpResult(txn.Id, url));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "VNPay URL generation failed TxnRef={TxnRef}", txnRef);
            txn.MarkFailed($"Gateway error: {ex.Message}");
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<InitiateTopUpResult>(
                Error.Failure("TopUp.GatewayError", "Could not connect to payment provider."));
        }
    }
}