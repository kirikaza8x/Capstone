using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Organizers.Handlers;

public class RejectOrganizerProfileCommandHandler
    : ICommandHandler<RejectOrganizerProfileCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserUnitOfWork _unitOfWork;

    public RejectOrganizerProfileCommandHandler(
        IUserRepository userRepository,
        IUserUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RejectOrganizerProfileCommand command,
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
                Error.Failure("Organizer.NoPending", "No pending profile to reject"));
        }

        try
        {
            user.RejectProfile(command.Reason);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(
                Error.Failure("Organizer.Reject.Invalid", ex.Message));
        }

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}