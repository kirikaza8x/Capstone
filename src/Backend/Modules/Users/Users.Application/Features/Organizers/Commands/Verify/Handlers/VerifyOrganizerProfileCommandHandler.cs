using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Commands;
using Users.Domain.Repositories;
using Users.Domain.UOW;

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

        user.VerifyOrganizerProfile();

        _userRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}