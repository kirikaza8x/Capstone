using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher,
    IUserUnitOfWork unitOfWork
) : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await userRepository.GetUserByMailOrUserNameAsync(
            [command.UserName, command.Email],
            cancellationToken);

        if (existingUser != null)
        {
            var msg = existingUser.Email == command.Email 
                ? "Email already in use." 
                : "Username taken.";
            return Result.Failure<Guid>(Error.Conflict("User.Exists", msg));
        }

        // Get or create role (use actual value, not nameof)
        var role = await roleRepository.GetByRoleNameAsync(command.Role.ToString(), cancellationToken)
                   ?? Role.Create(command.Role.ToString(), string.Empty);

        // Hash password
        var passwordHash = passwordHasher.HashPassword(command.Password);

        // Create user
        var user = User.Create(
            email: command.Email,
            userName: command.UserName,
            passwordHash: passwordHash,
            firstName: command.FirstName,
            lastName: command.LastName,
            phoneNumber: command.PhoneNumber,
            address: command.Address
        );

        user.AssignRole(role);

        // Persist
        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
