using Shared.Application.Abstractions.Messaging;

namespace Users.Application.Features.Users.Commands.Records;

public record RegisterUserCommand(
    string Email,
    string UserName,
    string Password,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? Address) : ICommand<Guid>;