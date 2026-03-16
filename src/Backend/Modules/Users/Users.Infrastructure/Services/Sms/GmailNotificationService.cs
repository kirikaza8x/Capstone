using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Users.Application.Abstractions.Sms;

namespace Users.Infrastructure.Services.Sms
{
    public class GmailNotificationService : IUserNotificationService
    {
        private readonly ILogger<GmailNotificationService> _logger;
        private readonly GmailSmsConfigs _configs;
        public GmailNotificationService(ILogger<GmailNotificationService> logger, IOptions<GmailSmsConfigs> configs)
        {
            _logger = logger;
            _configs = configs.Value;

        }

        public async Task SendOtpAsync(Guid userId, string email, string otpCode, CancellationToken cancellationToken = default)
        {
            var subject = "Your OTP Code";
            var body = $"Hello,\n\nYour OTP code is: {otpCode}\nIt will expire in 5 minutes.\n\nBest regards,\nUser Service";

            await SendEmailAsync(email, subject, body, cancellationToken);
        }

        public async Task SendPasswordChangedAlertAsync(Guid userId, string email, CancellationToken cancellationToken = default)
        {
            var subject = "Password Changed";
            var body = $"Hello,\n\nYour password was successfully changed at {DateTime.UtcNow}.\nIf this wasn’t you, please contact support immediately.";

            await SendEmailAsync(email, subject, body, cancellationToken);
        }

        private async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken)
        {
            using var client = new SmtpClient(_configs.SmtpServer, _configs.SmtpPort)
            {
                Credentials = new NetworkCredential(_configs.SenderEmail, _configs.SenderPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage(_configs.SenderEmail, to, subject, body);

            try
            {
                await client.SendMailAsync(mailMessage, cancellationToken);
                _logger.LogInformation("Email sent to {Recipient}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}", to);
                throw;
            }
        }
    }
}
