using Shared.Application.Abstractions.Notifications;
using Users.Application.Abstractions.Sms;

namespace Users.Infrastructure.Services.Sms
{
    public sealed class GmailNotificationService(
        IEmailSender emailSender) : IUserNotificationService
    {
        public async Task SendOtpAsync(Guid userId, string email, string otpCode, CancellationToken cancellationToken = default)
        {
            var subject = "Your OTP Code";
            var body = $"Hello,\n\nYour OTP code is: {otpCode}\nIt will expire in 5 minutes.\n\nBest regards,\nUser Service";

            await emailSender.SendAsync(new EmailMessage(email, subject, body), cancellationToken);
        }

        public async Task SendPasswordChangedAlertAsync(Guid userId, string email, CancellationToken cancellationToken = default)
        {
            var subject = "Password Changed";
            var body = $"Hello,\n\nYour password was successfully changed at {DateTime.UtcNow}.\nIf this wasn’t you, please contact support immediately.";

            await emailSender.SendAsync(new EmailMessage(email, subject, body), cancellationToken);
        }
    }
}
