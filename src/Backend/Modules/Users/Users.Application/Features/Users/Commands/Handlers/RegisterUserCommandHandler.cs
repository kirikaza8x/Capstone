using AutoMapper;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, UserResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
        }

        public async Task<Result<UserResponseDto>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var request = command.RegisterRequest;

            var existingUser = await _userRepository.GetUserByMailOrUserName(
                new[] { request.UserName, request.Email },
                cancellationToken);

            if (existingUser != null)
            {
                string errorMessage = existingUser.Email == request.Email && existingUser.UserName == request.UserName
                    ? "A user with the same email and username already exists."
                    : existingUser.Email == request.Email
                        ? "This email is already in use."
                        : "This username is already taken.";

                return Result.Failure<UserResponseDto>(
                    Error.Conflict("User.Exists", errorMessage)
                );
            }

            var passwordHash = _passwordHasher.HashPassword(request.Password);

            var user = User.Create(
                email: request.Email,
                userName: request.UserName,
                passwordHash: passwordHash,
                firstName: request.FirstName,
                lastName: request.LastName,
                phoneNumber: request.PhoneNumber,
                address: request.Address
            );

            _userRepository.Add(user);

            var response = _mapper.Map<UserResponseDto>(user);

            return Result.Success(response);
        }
    }
}
