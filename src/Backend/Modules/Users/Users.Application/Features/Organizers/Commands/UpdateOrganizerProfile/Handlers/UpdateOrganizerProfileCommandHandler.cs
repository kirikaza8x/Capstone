using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Commands;
using Users.Domain.Repositories;
using Users.Domain.UOW;
using Users.Domain.ValueObjects;

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