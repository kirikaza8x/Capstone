using AutoMapper;
using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUserUnitOfWork unitOfWork,
    IValidator<RegisterUserCommand> validator
) : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        // 1. Validation
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<Guid>(
                Error.Validation("Register.Validation", validationResult.Errors.First().ErrorMessage));
        }

        // 2. Uniqueness Check
        var existingUser = await userRepository.GetUserByMailOrUserNameAsync(
            [command.UserName, command.Email],
            cancellationToken);

        if (existingUser != null)
        {
            var msg = existingUser.Email == command.Email ? "Email already in use." : "Username taken.";
            return Result.Failure<Guid>(Error.Conflict("User.Exists", msg));
        }

        // 3. Creation
        var passwordHash = passwordHasher.HashPassword(command.Password);

        var user = User.Create(
            email: command.Email,
            userName: command.UserName,
            passwordHash: passwordHash,
            firstName: command.FirstName,
            lastName: command.LastName,
            phoneNumber: command.PhoneNumber,
            address: command.Address
        );

        userRepository.Add(user);
        // 4. Persistence
        // This triggers the dispatch of UserCreatedEvent
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}