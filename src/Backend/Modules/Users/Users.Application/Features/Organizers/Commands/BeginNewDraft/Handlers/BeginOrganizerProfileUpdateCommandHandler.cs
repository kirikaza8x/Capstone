using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Domain.Repositories;
using Users.Domain.UOW;

public class BeginOrganizerProfileUpdateCommandHandler
    : ICommandHandler<BeginOrganizerProfileUpdateCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserUnitOfWork _unitOfWork;

    public BeginOrganizerProfileUpdateCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IUserUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        BeginOrganizerProfileUpdateCommand command,
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

        user.BeginProfileUpdate();

        _userRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
