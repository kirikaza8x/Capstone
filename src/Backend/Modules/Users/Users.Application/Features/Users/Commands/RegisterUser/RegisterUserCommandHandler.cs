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
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be valid.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required.")
                .MaximumLength(100).WithMessage("Username must not exceed 100 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.Address)
                .MaximumLength(256)
                .When(x => !string.IsNullOrWhiteSpace(x.Address));
        }
    }

    public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, UserResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;
        private readonly IValidator<RegisterUserCommand> _validator;        
        // private readonly IUserUnitOfWork _unitOfWork; 

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IUserUnitOfWork unitOfWork, 
            IMapper mapper,
            IValidator<RegisterUserCommand> validator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _validator = validator;
            // _unitOfWork = unitOfWork;
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

            var existingUser = await _userRepository.GetUserByMailOrUserName(
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

            _userRepository.Add(user);
            // await _unitOfWork.SaveChangesAsync(cancellationToken);
            var response = _mapper.Map<UserResponseDto>(user);

            return Result.Success(response);
        }
    }
}
