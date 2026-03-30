using Payments.Application.Features.WithdrawalRequests.Commands;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.WithdrawalRequests.Handlers;

public class CreateWithdrawalRequestCommandHandler
    : ICommandHandler<CreateWithdrawalRequestCommand, Guid>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentUnitOfWork _unitOfWork;

    public CreateWithdrawalRequestCommandHandler(
        IWalletRepository walletRepository,
        IWithdrawalRequestRepository withdrawalRequestRepository,
        ICurrentUserService currentUserService,
        IPaymentUnitOfWork unitOfWork)
    {
        _walletRepository             = walletRepository;
        _withdrawalRequestRepository  = withdrawalRequestRepository;
        _currentUserService           = currentUserService;
        _unitOfWork                   = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateWithdrawalRequestCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // 1. Ensure no active request already exists
        var hasActive = await _withdrawalRequestRepository
            .HasActiveRequestAsync(userId, cancellationToken);

        if (hasActive)
            return Result.Failure<Guid>(
                Error.Conflict(
                    "WithdrawalRequest.AlreadyActive",
                    "You already have a pending or approved withdrawal request. " +
                    "Please wait for it to be processed before submitting a new one."));

        // 2. Load wallet and validate balance
        var wallet = await _walletRepository
            .GetByUserIdAsync(userId, cancellationToken);

        if (wallet == null)
            return Result.Failure<Guid>(
                Error.NotFound("Wallet.NotFound", "Wallet not found."));

        if (wallet.Balance < command.Amount)
            return Result.Failure<Guid>(
                Error.Validation(
                    "WithdrawalRequest.InsufficientFunds",
                    $"Insufficient balance. Available: {wallet.Balance:N0}, Requested: {command.Amount:N0}."));

        // 3. Create the request
        var request = WithdrawalRequest.Create(
            userId:            userId,
            walletId:          wallet.Id,
            bankAccountNumber: command.BankAccountNumber,
            bankName:          command.BankName,
            amount:            command.Amount,
            notes:             command.Notes);

        _withdrawalRequestRepository.Add(request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(request.Id);
    }
}