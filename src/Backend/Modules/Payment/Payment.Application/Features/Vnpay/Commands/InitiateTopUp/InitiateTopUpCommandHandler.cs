using Microsoft.Extensions.Logging;
using Payment.Domain.Enums;
using Payments.Application.Abstractions;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Payments.Commands.InitiateTopUp;

public class InitiateTopUpCommandHandler(
    IVnPayService vnPayService,
    ICurrentUserService currentUserService,
    IPaymentTransactionRepository transactionRepository,
    IWalletRepository walletRepository,
    IPaymentUnitOfWork unitOfWork,
    ILogger<InitiateTopUpCommandHandler> logger)
    : ICommandHandler<InitiateTopUpCommand, InitiateTopUpResult>
{
    public async Task<Result<InitiateTopUpResult>> Handle(
        InitiateTopUpCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var ipAddress = currentUserService.IpAddress ?? "127.0.0.1";

        // 1. Resolve or create wallet
        var wallet = await walletRepository
            .GetByUserIdAsync(userId, cancellationToken);

        if (wallet == null)
        {
            wallet = Wallet.Create(userId);
            walletRepository.Add(wallet);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Wallet auto-created for UserId={UserId}", userId);
        }
        else if (wallet.Status == WalletStatus.Closed)
        {
            return Result.Failure<InitiateTopUpResult>(
                Error.Failure("TopUp.WalletClosed",
                    "Cannot top up a closed wallet."));
        }
        else if (wallet.Status == WalletStatus.Suspended)
        {
            wallet.ChangeStatus(WalletStatus.Active);
            walletRepository.Update(wallet);
        }

        // 2. Create transaction
        var txnRef = Guid.NewGuid().ToString("N");
        var orderInfo = command.Description
                        ?? $"Wallet top-up {command.Amount:N0} VND";

        var txn = PaymentTransaction.CreateWalletTopUp(
            userId, wallet.Id, command.Amount, orderInfo, txnRef, ipAddress);

        transactionRepository.Add(txn);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 3. Generate VNPay URL
        try
        {
            var url = vnPayService.CreatePaymentUrl(
                command.Amount, txnRef, orderInfo, ipAddress);

            logger.LogInformation(
                "TopUp initiated: UserId={UserId}, Amount={Amount}, TxnRef={TxnRef}",
                userId, command.Amount, txnRef);

            return Result.Success(new InitiateTopUpResult(txn.Id, url));
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "VNPay URL generation failed TxnRef={TxnRef}", txnRef);

            txn.MarkFailed($"Gateway error: {ex.Message}");
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<InitiateTopUpResult>(
                Error.Failure("TopUp.GatewayError",
                    "Could not connect to payment provider."));
        }
    }
}
