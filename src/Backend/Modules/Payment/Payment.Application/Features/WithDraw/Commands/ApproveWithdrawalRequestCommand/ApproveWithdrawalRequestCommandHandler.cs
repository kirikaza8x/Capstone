using Payments.Application.Features.WithdrawalRequests.Commands;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.WithdrawalRequests.Handlers;

public class ApproveWithdrawalRequestCommandHandler
    : ICommandHandler<ApproveWithdrawalRequestCommand>
{
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IPaymentUnitOfWork _unitOfWork;

    public ApproveWithdrawalRequestCommandHandler(
        IWithdrawalRequestRepository withdrawalRequestRepository,
        IWalletRepository walletRepository,
        IPaymentUnitOfWork unitOfWork)
    {
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _walletRepository            = walletRepository;
        _unitOfWork                  = unitOfWork;
    }

    public async Task<Result> Handle(
        ApproveWithdrawalRequestCommand command,
        CancellationToken cancellationToken)
    {
        var request = await _withdrawalRequestRepository
            .GetByIdAsync(command.RequestId, cancellationToken);

        if (request == null)
            return Result.Failure(
                Error.NotFound("WithdrawalRequest.NotFound", "Withdrawal request not found."));

        // Load wallet to debit (hold) the funds on approval
        var wallet = await _walletRepository
            .GetByUserIdAsync(request.UserId, cancellationToken);

        if (wallet == null)
            return Result.Failure(
                Error.NotFound("Wallet.NotFound", "Wallet not found."));

        try
        {
            // Debit the wallet immediately — funds are held until transfer is done
            var txn = wallet.Debit(
                request.Amount,
                note: $"Withdrawal hold — request {request.Id}");

            request.Approve(txn.Id, command.AdminNote);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(
                Error.Failure("WithdrawalRequest.Approve.Invalid", ex.Message));
        }

        _walletRepository.Update(wallet);
        _withdrawalRequestRepository.Update(request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}