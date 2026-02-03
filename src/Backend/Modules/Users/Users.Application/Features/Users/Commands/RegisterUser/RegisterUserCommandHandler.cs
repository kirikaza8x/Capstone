using Shared.Application.Abstractions.Authentication;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using FluentValidation;
using AutoMapper;
using Users.Domain.UOW;

namespace Users.Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, UserResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;
        private readonly IValidator<RegisterUserCommand> _validator;
        private readonly IUserUnitOfWork _unitOfWork;

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordHasher passwordHasher,
            IUserUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<RegisterUserCommand> validator)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UserResponseDto>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var firstError = validationResult.Errors.First();
                return Result.Failure<UserResponseDto>(
                    Error.Validation("Register.Validation", firstError.ErrorMessage)
                );
            }

            var existingUser = await _userRepository.GetUserByMailOrUserNameAsync(
                new[] { command.UserName, command.Email },
                cancellationToken);

            if (existingUser != null)
            {
                string errorMessage = existingUser.Email == command.Email && existingUser.UserName == command.UserName
                    ? "A user with the same email and username already exists."
                    : existingUser.Email == command.Email
                        ? "This email is already in use."
                        : "This username is already taken.";

                return Result.Failure<UserResponseDto>(
                    Error.Conflict("User.Exists", errorMessage)
                );
            }

            var passwordHash = _passwordHasher.HashPassword(command.Password);

            var user = User.Create(
                email: command.Email,
                userName: command.UserName,
                passwordHash: passwordHash,
                firstName: command.FirstName,
                lastName: command.LastName,
                phoneNumber: command.PhoneNumber,
                address: command.Address
            );

            var role = Role.Create("User", "Default role with standard permissions.");

            await _userRepository.RegisterAsync(user, role, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<UserResponseDto>(user);
            return Result.Success(response);
        }
    }
}
