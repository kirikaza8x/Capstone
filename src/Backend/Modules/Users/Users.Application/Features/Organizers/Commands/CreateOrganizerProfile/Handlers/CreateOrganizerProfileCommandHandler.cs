using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Commands;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Organizers.Handlers;

public class CreateOrganizerProfileCommandHandler
    : ICommandHandler<CreateOrganizerProfileCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserUnitOfWork _unitOfWork;

    public CreateOrganizerProfileCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IUserUnitOfWork unitOfWork
        )
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateOrganizerProfileCommand command,
        CancellationToken cancellationToken)
    {



        // Get current user
        var userId = _currentUserService.UserId;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<Guid>(
                Error.NotFound("User.NotFound", "User not found"));
        }

        user.CreateOrganizerProfile(command.Type);

        _userRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get the newly created draft profile
        var draftProfile = user.PendingProfile;

        if (draftProfile == null)
        {
            return Result.Failure<Guid>(
                Error.Failure("Organizer.Create.Failed", "Failed to create organizer profile."));
        }

        return Result.Success(draftProfile.Id);
    }
}
