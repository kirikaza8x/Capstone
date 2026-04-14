using AutoMapper;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Enums;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Users.Commands.Handlers;

public class UpdateStatusCommandHandler : ICommandHandler<UpdateStatusCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserUnitOfWork _unitOfWork;

    private readonly IMapper _mapper;

    public UpdateStatusCommandHandler(
        IUserRepository userRepository,
        IUserUnitOfWork unitOfWork
       ,
        IMapper mapper)
    {
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
    }

    public async Task<Result> Handle(UpdateStatusCommand command, CancellationToken cancellationToken)
    {

        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user == null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found."));


        switch (command.UserStatus)
        {
            case UserStatus.Active:
                user.Activate();
                break;
            case UserStatus.Inactive:
                user.Deactivate(); ;
                break;
            case UserStatus.Banned:
                user.Ban();
                break;
            default:
                return Result.Failure(Error.Validation("User.InvalidStatus", "Invalid user status."));
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();

    }
}
