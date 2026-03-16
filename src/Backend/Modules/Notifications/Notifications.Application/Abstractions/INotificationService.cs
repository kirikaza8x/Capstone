namespace Notifications.Application.Abstractions;

public interface INotificationService
{
    Task SendRawEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default);

    Task SendOtpAsync(
        string to,
        string otpCode,
        CancellationToken cancellationToken = default);

    Task SendPasswordChangedAlertAsync(
        string to,
        DateTime changedAtUtc,
        CancellationToken cancellationToken = default);
}