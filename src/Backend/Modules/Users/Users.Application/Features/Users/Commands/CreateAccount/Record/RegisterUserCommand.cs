using Shared.Application.Abstractions.Messaging;
using Users.Domain.Enums;

namespace Users.Application.Features.Users.Commands.Records;

public record CreateUserCommand(
    string Email,
    string UserName,
    string Password,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? Address,
    RolesType Role = RolesType.Staff
    ) : ICommand<Guid>;
