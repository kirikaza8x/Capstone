using Payments.Application.Features.WithdrawalRequests.Commands;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.WithdrawalRequests.Handlers;

public class RejectWithdrawalRequestCommandHandler
    : ICommandHandler<RejectWithdrawalRequestCommand>
{
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IPaymentUnitOfWork _unitOfWork;

    public RejectWithdrawalRequestCommandHandler(
        IWithdrawalRequestRepository withdrawalRequestRepository,
        IPaymentUnitOfWork unitOfWork)
    {
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _unitOfWork                  = unitOfWork;
    }

    public async Task<Result> Handle(
        RejectWithdrawalRequestCommand command,
        CancellationToken cancellationToken)
    {
        var request = await _withdrawalRequestRepository
            .GetByIdAsync(command.RequestId, cancellationToken);

        if (request == null)
            return Result.Failure(
                Error.NotFound("WithdrawalRequest.NotFound", "Withdrawal request not found."));

        try
        {
            // No balance change needed — funds were never debited at Pending stage
            request.Reject(command.AdminNote);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(
                Error.Failure("WithdrawalRequest.Reject.Invalid", ex.Message));
        }

        _withdrawalRequestRepository.Update(request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}