using Events.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Notifications;
using Users.PublicApi.PublicApi;

namespace Notifications.Application.IntegrationEventHandlers;

internal sealed class EventSuspendedIntegrationEventHandler(
    IUserPublicApi userPublicApi,
    IEmailSender emailSender,
    ILogger<EventSuspendedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<EventSuspendedIntegrationEvent>
{
    public async Task Handle(EventSuspendedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var organizer = await userPublicApi.GetByIdAsync(integrationEvent.OrganizerId, cancellationToken);
        if (organizer is null || string.IsNullOrWhiteSpace(organizer.Email))
        {
            logger.LogWarning("Organizer not found/email empty. EventId: {EventId}", integrationEvent.EventId);
            return;
        }

        var subject = $"[AIPromo] Event bị tạm dừng: {integrationEvent.EventTitle}";
        var body =
            $"Xin chào {organizer.FullName},\n\n" +
            $"Event \"{integrationEvent.EventTitle}\" đã bị tạm dừng.\n" +
            $"Lý do: {integrationEvent.SuspensionReason}\n" +
            $"Hạn chỉnh sửa và gửi lại: {integrationEvent.SuspendedUntilAtUtc:yyyy-MM-dd HH:mm:ss} UTC.\n\n" +
            "Vui lòng cập nhật thông tin event và submit lại để được duyệt.\n\n" +
            "AIPromo";

        await emailSender.SendAsync(new EmailMessage(organizer.Email, subject, body), cancellationToken);
    }
}
