using Shared.Application.Abstractions.Messaging;

namespace Users.Application.Features.Users.Commands.Records;

/// <summary>
/// Command to finalize the password reset using the received OTP.
/// </summary>
/// <param name="Email">The user's email (used to locate the aggregate).</param>
/// <param name="OtpCode">The 6-digit code received via email.</param>
/// <param name="NewPassword">The plain-text new password (to be hashed in the handler).</param>
public record ResetPasswordCommand(
    string Email,
    string OtpCode,
    string NewPassword) : ICommand<Guid>;
