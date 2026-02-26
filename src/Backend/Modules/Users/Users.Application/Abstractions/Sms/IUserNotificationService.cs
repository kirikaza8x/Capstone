namespace Users.Application.Abstractions.Sms
{
    /// <summary>
    /// Contract for sending user notifications (email, SMS, etc.)
    /// </summary>
    public interface IUserNotificationService
    {
        Task SendOtpAsync(Guid userId, string email, string otpCode, CancellationToken cancellationToken = default);
        Task SendPasswordChangedAlertAsync(Guid userId, string email, CancellationToken cancellationToken = default);
    }
}
