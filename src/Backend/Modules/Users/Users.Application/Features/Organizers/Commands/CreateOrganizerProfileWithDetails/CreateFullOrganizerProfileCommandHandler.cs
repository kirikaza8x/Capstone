using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Domain.Repositories;
using Users.Domain.UOW;
using Users.Domain.ValueObjects; // Added to access OrganizerBusinessInfo & OrganizerBankInfo

namespace Users.Application.Features.Organizers.Handlers;

public class CreateFullOrganizerProfileCommandHandler 
    : ICommandHandler<CreateFullOrganizerProfileCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserUnitOfWork _unitOfWork;

    public CreateFullOrganizerProfileCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IUserUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateFullOrganizerProfileCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Get current user
        var userId = _currentUserService.UserId;
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<Guid>(
                Error.NotFound("User.NotFound", "User not found"));
        }

        // 2. Map Command DTOs to Domain Value Objects
        // Assuming your Value Objects have constructors or initialization logic
        var businessInfo = new OrganizerBusinessInfo(
            command.BusinessInfo.Logo,
            command.BusinessInfo.DisplayName,
            command.BusinessInfo.Description,
            command.BusinessInfo.Address,
            command.BusinessInfo.SocialLink,
            command.BusinessInfo.BusinessType,
            command.BusinessInfo.TaxCode,
            command.BusinessInfo.IdentityNumber,
            command.BusinessInfo.CompanyName
        );

        // Assuming OrganizerBankInfo follows the same immutable pattern
        var bankInfo = new OrganizerBankInfo(
            command.BankInfo.AccountName,
            command.BankInfo.AccountNumber,
            command.BankInfo.BankCode,
            command.BankInfo.Branch
        );

        // 3. Execute Domain Logic
        user.CreateFullOrganizerProfile(command.Type, businessInfo, bankInfo);

        // 4. Save Changes
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Get the newly created draft profile
        // Note: Used DraftProfile here because the factory sets Status = OrganizerStatus.Draft
        var draftProfile = user.DraftProfile;

        if (draftProfile == null)
        {
            return Result.Failure<Guid>(
                Error.Failure("Organizer.Create.Failed", "Failed to create full organizer profile."));
        }

        return Result.Success(draftProfile.Id);
    }
}