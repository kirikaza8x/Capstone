using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Commands;
using Users.Domain.Repositories;
using Users.Domain.UOW;
using Users.Domain.ValueObjects;

namespace Users.Application.Features.Organizers.Handlers;

public class UpdateOrganizerProfileCommandHandler
    : ICommandHandler<UpdateOrganizerProfileCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserUnitOfWork _unitOfWork;

    public UpdateOrganizerProfileCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IUserUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateOrganizerProfileCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return Result.Failure(
                Error.NotFound("User.NotFound", "User not found"));
        }

        if (user.DraftProfile == null && user.PublishedProfile != null)
        {
            user.BeginProfileUpdate();
        }

        var profile = user.DraftProfile;

        if (profile == null)
        {
            return Result.Failure(
                Error.Conflict("Organizer.NoDraft", "No draft profile available for editing"));
        }

        var businessInfo = new OrganizerBusinessInfo(
            command.Logo,
            command.DisplayName,
            command.Description,
            command.Address,
            command.SocialLink,
            command.BusinessType,
            command.TaxCode,
            command.IdentityNumber,
            command.CompanyName
        );

        user.UpdateOrganizerProfile(businessInfo);

        _userRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
