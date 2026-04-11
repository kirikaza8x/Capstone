using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Commands;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Organizers.Handlers;

public class VerifyOrganizerProfileCommandHandler
    : ICommandHandler<VerifyOrganizerProfileCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserUnitOfWork _unitOfWork;

    public VerifyOrganizerProfileCommandHandler(
        IUserRepository userRepository,
        IUserUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        VerifyOrganizerProfileCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithOrganizerProfileAsync(
            command.UserId,
            cancellationToken);

        if (user == null)
        {
            return Result.Failure(
                Error.NotFound("User.NotFound", "User not found"));
        }

        if (user.PendingProfile == null)
        {
            return Result.Failure(
                Error.Failure("Organizer.NoPending", "No pending profile to verify"));
        }

        try
        {
            user.VerifyProfile();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(
                Error.Failure("Organizer.Verify.Invalid", ex.Message));
        }

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}