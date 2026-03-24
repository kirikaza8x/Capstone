using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Organizers.Handlers;

public class SubmitOrganizerProfileCommandHandler
    : ICommandHandler<SubmitOrganizerProfileCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserUnitOfWork _unitOfWork;

    public SubmitOrganizerProfileCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IUserUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        SubmitOrganizerProfileCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithOrganizerProfileAsync(
            _currentUserService.UserId,
            cancellationToken);

        if (user == null)
        {
            return Result.Failure(
                Error.NotFound("User.NotFound", "User not found"));
        }

        if (user.DraftProfile == null)
        {
            return Result.Failure(
                Error.Failure("Organizer.NoDraft", "No draft profile to submit"));
        }

        try
        {
            user.SubmitProfile();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(
                Error.Failure("Organizer.Submit.Invalid", ex.Message));
        }

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}