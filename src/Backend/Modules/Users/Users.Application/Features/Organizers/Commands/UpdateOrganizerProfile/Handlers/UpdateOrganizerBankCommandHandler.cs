using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Commands;
using Users.Domain.Repositories;
using Users.Domain.UOW;
using Users.Domain.ValueObjects;

public class UpdateOrganizerBankCommandHandler
    : ICommandHandler<UpdateOrganizerBankCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserUnitOfWork _unitOfWork;

    public UpdateOrganizerBankCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IUserUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateOrganizerBankCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(
            _currentUserService.UserId,
            cancellationToken);

        if (user == null)
        {
            return Result.Failure(
                Error.NotFound("User.NotFound", "User not found"));
        }

        if (user.OrganizerProfile == null)
        {
            return Result.Failure(
                Error.NotFound("Organizer.NotFound", "Organizer profile not found"));
        }

        var bankInfo = new OrganizerBankInfo(
            command.AccountName,
            command.AccountNumber,
            command.BankCode,
            command.Branch
        );

        user.UpdateOrganizerBank(bankInfo);

        _userRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}