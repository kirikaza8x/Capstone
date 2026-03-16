using Notifications.Application.Abstractions;

namespace Notifications.Application.Services;

internal sealed class NotificationService(
    IEmailSender emailSender) : INotificationService
{
    public Task SendRawEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        return emailSender.SendAsync(to, subject, body, cancellationToken);
    }

    public Task SendOtpAsync(
        string to,
        string otpCode,
        CancellationToken cancellationToken = default)
    {
        var subject = "Your OTP Code";
        var body = $"Hello,\n\nYour OTP code is: {otpCode}\nIt will expire in 5 minutes.\n\nBest regards,\nAIPromo";

        return emailSender.SendAsync(to, subject, body, cancellationToken);
    }

    public Task SendPasswordChangedAlertAsync(
        string to,
        DateTime changedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var subject = "Password Changed";
        var body = $"Hello,\n\nYour password was successfully changed at {changedAtUtc:u}.\nIf this wasn’t you, please contact support immediately.";

        return emailSender.SendAsync(to, subject, body, cancellationToken);
    }
}