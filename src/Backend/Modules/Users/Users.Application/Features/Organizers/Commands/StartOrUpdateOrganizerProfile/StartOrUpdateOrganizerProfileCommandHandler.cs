using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Storage;
using Shared.Domain.Abstractions;
using Users.Application.Storage;
using Users.Domain.Repositories;
using Users.Domain.UOW;
using Users.Domain.ValueObjects; 

namespace Users.Application.Features.Organizers.Handlers;

public class StartOrUpdateOrganizerProfileCommandHandler
    : ICommandHandler<StartOrUpdateOrganizerProfileCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    private readonly IStorageService _storageService;
    private readonly IUserUnitOfWork _unitOfWork;

    public StartOrUpdateOrganizerProfileCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IStorageService storageService,
        IUserUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
    StartOrUpdateOrganizerProfileCommand command,
    CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var user = await _userRepository
            .GetByIdWithOrganizerProfileAsync(userId, cancellationToken);

        

        // await using var stream = command.LogoFile.OpenReadStream();
        // var folder = $"{StoragePath.OrganizerProfileImages}/{command.user}";

        // var imageUrl = await storageService.UploadAsync(
        //     stream,
        //     command.LogoFile.FileName,
        //     command.File.ContentType,
        //     folder,
        //     cancellationToken);

        if (user == null)
        {
            return Result.Failure<Guid>(
                Error.NotFound("User.NotFound", "User not found"));
        }


        if (user.PendingProfile != null)
        {
            return Result.Failure<Guid>(
                Error.Conflict(
                    "Organizer.Pending",
                    "Your profile is under review."));
        }

        if (string.IsNullOrWhiteSpace(command.BusinessInfo.DisplayName))
        {
            return Result.Failure<Guid>(
                Error.Validation(
                    "Organizer.DisplayName.Required",
                    "Display name is required."));
        }

        if (string.IsNullOrWhiteSpace(command.BankInfo.AccountNumber))
        {
            return Result.Failure<Guid>(
                Error.Validation(
                    "Organizer.Bank.Required",
                    "Bank account is required."));
        }

        
        var businessInfo = new OrganizerBusinessInfo(
            command.BusinessInfo.DisplayName,
            command.BusinessInfo.Description,
            command.BusinessInfo.Address,
            command.BusinessInfo.SocialLink,
            command.BusinessInfo.BusinessType,
            command.BusinessInfo.TaxCode,
            command.BusinessInfo.IdentityNumber,
            command.BusinessInfo.CompanyName
        );

        var bankInfo = new OrganizerBankInfo(
            command.BankInfo.AccountName,
            command.BankInfo.AccountNumber,
            command.BankInfo.BankCode,
            command.BankInfo.Branch
        );

        
        user.StartOrUpdateOrganizerProfile(
            command.Type,
            businessInfo,
            bankInfo);

        if(command.LogoFile != null)
        {
            await using var stream = command.LogoFile.OpenReadStream();
            var folder = $"{StoragePath.OrganizerProfileImages}/{user.Id}";

            var imageUrl = await _storageService.UploadAsync(
                stream,
                command.LogoFile.FileName,
                command.LogoFile.ContentType,
                folder,
                cancellationToken);

            user.UpdateDraftLogo(imageUrl);
        }

            

        // --------------------
        // Persist
        // --------------------
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var draft = user.DraftProfile;

        if (draft == null)
        {
            return Result.Failure<Guid>(
                Error.Failure(
                    "Organizer.Profile.Missing",
                    "Draft profile not found after operation."));
        }

        return Result.Success(draft.Id);
    }
}