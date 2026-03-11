using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Domain.Repositories;
using Users.Domain.UOW;

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
        var user = await _userRepository.GetByIdAsync(
            command.UserId,
            cancellationToken);

        if (user == null)
        {
            return Result.Failure(
                Error.NotFound("User.NotFound", "User not found"));
        }

        if (user.OrganizerProfiles == null)
        {
            return Result.Failure(
                Error.NotFound("Organizer.NotFound", "Organizer profile not found"));
        }

        user.RejectOrganizerProfile(command.Reason);

        _userRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}