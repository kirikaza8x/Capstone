using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notifications.Application.Abstractions;
using Notifications.Infrastructure.Configs;

namespace Notifications.Infrastructure.Services;

internal sealed class GmailEmailSender(
    ILogger<GmailEmailSender> logger,
    IOptions<GmailNotificationConfigs> configs) : IEmailSender
{
    private readonly GmailNotificationConfigs _configs = configs.Value;

    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        using var client = new SmtpClient(_configs.SmtpServer, _configs.SmtpPort)
        {
            Credentials = new NetworkCredential(_configs.SenderEmail, _configs.SenderPassword),
            EnableSsl = true
        };

        using var mailMessage = new MailMessage(_configs.SenderEmail, to, subject, body);

        try
        {
            await client.SendMailAsync(mailMessage, cancellationToken);
            logger.LogInformation("Email sent to {Recipient}", to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Recipient}", to);
            throw;
        }
    }
}