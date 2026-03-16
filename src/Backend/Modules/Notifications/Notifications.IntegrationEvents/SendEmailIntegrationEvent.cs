using Shared.Application.Abstractions.EventBus;

namespace Notifications.IntegrationEvents;

public sealed record SendEmailIntegrationEvent : IntegrationEvent
{
    public string To { get; }
    public string Subject { get; }
    public string Body { get; }

    public SendEmailIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        string to,
        string subject,
        string body)
        : base(id, occurredOnUtc)
    {
        To = to;
        Subject = subject;
        Body = body;
    }
}
