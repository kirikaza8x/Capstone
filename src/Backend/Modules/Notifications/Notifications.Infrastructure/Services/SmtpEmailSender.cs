using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notifications.Infrastructure.Configs;
using Shared.Application.Abstractions.Notifications;

namespace Notifications.Infrastructure.Services;

internal sealed class SmtpEmailSender(
    ILogger<SmtpEmailSender> logger,
    IOptions<SmtpOptions> options) : IEmailSender
{
    private readonly SmtpOptions _options = options.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        using var client = new SmtpClient(_options.SmtpServer, _options.SmtpPort)
        {
            Credentials = new NetworkCredential(_options.SenderEmail, _options.SenderPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage(_options.SenderEmail, message.To, message.Subject, message.Body)
        {
            IsBodyHtml = message.IsHtml
        };

        try
        {
            await client.SendMailAsync(mailMessage, cancellationToken);
            logger.LogInformation("Email sent to {Recipient}", message.To);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Recipient}", message.To);
            throw;
        }
    }
}