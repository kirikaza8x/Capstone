using Payments.Application.Features.WithdrawalRequests.Commands;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.WithdrawalRequests.Handlers;

public class CancelWithdrawalRequestCommandHandler
    : ICommandHandler<CancelWithdrawalRequestCommand>
{
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentUnitOfWork _unitOfWork;

    public CancelWithdrawalRequestCommandHandler(
        IWithdrawalRequestRepository withdrawalRequestRepository,
        ICurrentUserService currentUserService,
        IPaymentUnitOfWork unitOfWork)
    {
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _currentUserService          = currentUserService;
        _unitOfWork                  = unitOfWork;
    }

    public async Task<Result> Handle(
        CancelWithdrawalRequestCommand command,
        CancellationToken cancellationToken)
    {
        var request = await _withdrawalRequestRepository
            .GetByIdAsync(command.RequestId, cancellationToken);

        if (request == null)
            return Result.Failure(
                Error.NotFound("WithdrawalRequest.NotFound", "Withdrawal request not found."));

        if (request.UserId != _currentUserService.UserId)
            return Result.Failure(
                Error.Forbidden(
                    "WithdrawalRequest.Forbidden",
                    "You are not allowed to cancel this request."));

        try
        {
            request.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(
                Error.Failure("WithdrawalRequest.Cancel.Invalid", ex.Message));
        }

        _withdrawalRequestRepository.Update(request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}