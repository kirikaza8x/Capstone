using Notifications.Application.Abstractions;
using Notifications.IntegrationEvents;
using Shared.Application.Abstractions.EventBus;

namespace Notifications.Application.IntegrationEventHandlers;

internal sealed class SendEmailIntegrationEventHandler(
    INotificationService notificationService) : IntegrationEventHandler<SendEmailIntegrationEvent>
{
    public override async Task Handle(SendEmailIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        await notificationService.SendRawEmailAsync(
            integrationEvent.To,
            integrationEvent.Subject,
            integrationEvent.Body,
            cancellationToken);
    }
}