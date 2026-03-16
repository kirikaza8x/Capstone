using Shared.Infrastructure.Configs;

namespace Notifications.Infrastructure.Configs;

public sealed class GmailNotificationConfigs : ConfigBase
{
    public override string SectionName => "Sms:Gmail";
    public string SmtpServer { get; init; } = default!;
    public int SmtpPort { get; init; }
    public string SenderEmail { get; init; } = default!;
    public string SenderPassword { get; init; } = default!;
}