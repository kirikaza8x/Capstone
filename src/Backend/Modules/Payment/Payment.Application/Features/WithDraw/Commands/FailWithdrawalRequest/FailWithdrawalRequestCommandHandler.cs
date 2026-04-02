using Payments.Application.Features.WithdrawalRequests.Commands;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.WithdrawalRequests.Handlers;

public class FailWithdrawalRequestCommandHandler
    : ICommandHandler<FailWithdrawalRequestCommand>
{
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IPaymentUnitOfWork _unitOfWork;

    public FailWithdrawalRequestCommandHandler(
        IWithdrawalRequestRepository withdrawalRequestRepository,
        IWalletRepository walletRepository,
        IPaymentUnitOfWork unitOfWork)
    {
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        FailWithdrawalRequestCommand command,
        CancellationToken cancellationToken)
    {
        var request = await _withdrawalRequestRepository
            .GetByIdAsync(command.RequestId, cancellationToken);

        if (request == null)
            return Result.Failure(
                Error.NotFound("WithdrawalRequest.NotFound", "Withdrawal request not found."));

        var wallet = await _walletRepository
            .GetByUserIdAsync(request.UserId, cancellationToken);

        if (wallet == null)
            return Result.Failure(
                Error.NotFound("Wallet.NotFound", "Wallet not found."));

        try
        {
            // Mark the original debit transaction as failed
            var originalTxn = wallet.Transactions
                .FirstOrDefault(t => t.Id == request.WalletTransactionId);

            if (originalTxn == null)
                return Result.Failure(
                    Error.NotFound(
                        "WalletTransaction.NotFound",
                        "Linked wallet transaction not found."));

            originalTxn.MarkFailed($"Withdrawal failed — {command.AdminNote}");

            // Refund the held amount back to the wallet
            var refundTxn = wallet.Refund(
                request.Amount,
                note: $"Refund — failed withdrawal request {request.Id}");

            refundTxn.MarkCompleted();

            request.Fail(command.AdminNote);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(
                Error.Failure("WithdrawalRequest.Fail.Invalid", ex.Message));
        }

        _walletRepository.Update(wallet);
        _withdrawalRequestRepository.Update(request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}