
using Shared.Application.Messaging;

namespace Users.Application.Features.Users.Commands.Records;

/// <summary>
/// Command to initiate the password reset process by generating an OTP.
/// </summary>
/// <param name="Email">The email of the user who forgot their password.</param>
public record RequestPasswordResetCommand(string Email) : ICommand<Guid>;