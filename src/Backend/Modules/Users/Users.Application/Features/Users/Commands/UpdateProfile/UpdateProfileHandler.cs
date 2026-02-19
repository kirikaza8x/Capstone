using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Repositories;
using Users.Domain.UOW;
using FluentValidation;
using AutoMapper;
using Users.Domain.Enums;

namespace Users.Application.Features.Users.Commands.Handlers;

public class UpdateProfileCommandHandler : ICommandHandler<UpdateProfileCommand, UserProfileDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateProfileCommand> _validator;

    private readonly IMapper _mapper;

    public UpdateProfileCommandHandler(
        IUserRepository userRepository,
        IUserUnitOfWork unitOfWork,
        IValidator<UpdateProfileCommand> validator,
        IMapper mapper)
    {
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _validator = validator;
            _mapper = mapper;
        }
    }

    public async Task<Result<UserProfileDto>> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Errors.First();
            return Result.Failure<UserProfileDto>(
                Error.Validation("UpdateProfile.Validation", firstError.ErrorMessage)
            );
        }
        UserStatus.Active.ToString();
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<UserProfileDto>(Error.NotFound("User.NotFound", "User not found."));

        user.UpdateProfile(
            command.FirstName,
            command.LastName,
            command.Birthday,
            command.Gender,
            command.Phone,
            command.Address,
            command.Description,
            command.SocialLink,
            command.ProfileImageUrl
        );

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = user.Roles?.Select(r => r.Name).ToList() ?? new List<string>();

        var response = _mapper.Map<UserProfileDto>(user);
        return Result.Success(response);

    }
}
