using Events.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Notifications;
using Users.PublicApi.PublicApi;


namespace Notifications.Application.IntegrationEventHandlers;

internal sealed class EventReminderTriggeredIntegrationEventHandler(
    IUserPublicApi userPublicApi,
    IEmailSender emailSender,
    ILogger<EventReminderTriggeredIntegrationEventHandler> logger)
    : IIntegrationEventHandler<EventReminderTriggeredIntegrationEvent>
{
    public async Task Handle(
        EventReminderTriggeredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var organizer = await userPublicApi.GetByIdAsync(integrationEvent.OrganizerId, cancellationToken);
        var subject = $"[AIPromo] Reminder 24h - {integrationEvent.EventTitle}";
        var body =
            $"Xin chào {organizer.FullName},\n\n" +
            $"S? ki?n \"{integrationEvent.EventTitle}\" s? b?t ??u lúc {integrationEvent.EventStartAtUtc:yyyy-MM-dd HH:mm:ss} UTC.\n" +
            "Vui lòng ki?m tra l?i công tác chu?n b?.\n\n" +
            "AIPromo System";

        await emailSender.SendAsync(
            new EmailMessage(organizer.Email, subject, body),
            cancellationToken);

        logger.LogInformation(
            "Sent 24h reminder email to organizer {OrganizerId} for event {EventId}",
            integrationEvent.OrganizerId,
            integrationEvent.EventId);
    }
}
