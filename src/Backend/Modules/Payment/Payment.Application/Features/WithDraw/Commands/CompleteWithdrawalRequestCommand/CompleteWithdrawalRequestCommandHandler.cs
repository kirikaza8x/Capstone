using Payments.Application.Features.WithdrawalRequests.Commands;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.WithdrawalRequests.Handlers;

public class CompleteWithdrawalRequestCommandHandler
    : ICommandHandler<CompleteWithdrawalRequestCommand>
{
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IPaymentUnitOfWork _unitOfWork;

    public CompleteWithdrawalRequestCommandHandler(
        IWithdrawalRequestRepository withdrawalRequestRepository,
        IWalletRepository walletRepository,
        IPaymentUnitOfWork unitOfWork)
    {
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        CompleteWithdrawalRequestCommand command,
        CancellationToken cancellationToken)
    {
        var request = await _withdrawalRequestRepository
            .GetByIdAsync(command.RequestId, cancellationToken);

        if (request == null)
            return Result.Failure(
                Error.NotFound("WithdrawalRequest.NotFound", "Withdrawal request not found."));

        // Mark the linked wallet transaction as completed
        var wallet = await _walletRepository
            .GetByUserIdAsync(request.UserId, cancellationToken);

        if (wallet == null)
            return Result.Failure(
                Error.NotFound("Wallet.NotFound", "Wallet not found."));

        var walletTxn = wallet.Transactions
            .FirstOrDefault(t => t.Id == request.WalletTransactionId);

        if (walletTxn == null)
            return Result.Failure(
                Error.NotFound(
                    "WalletTransaction.NotFound",
                    "Linked wallet transaction not found."));

        try
        {
            walletTxn.MarkCompleted();
            request.Complete(command.AdminNote);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(
                Error.Failure("WithdrawalRequest.Complete.Invalid", ex.Message));
        }

        _walletRepository.Update(wallet);
        _withdrawalRequestRepository.Update(request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}