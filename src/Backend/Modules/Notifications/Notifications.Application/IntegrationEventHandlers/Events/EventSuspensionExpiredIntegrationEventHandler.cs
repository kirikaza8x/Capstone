using Events.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Notifications;
using Users.PublicApi.PublicApi;

namespace Notifications.Application.IntegrationEventHandlers.Events;

internal sealed class EventSuspensionExpiredIntegrationEventHandler(
    IUserPublicApi userPublicApi,
    IEmailSender emailSender,
    ILogger<EventSuspensionExpiredIntegrationEventHandler> logger)
    : IIntegrationEventHandler<EventSuspensionExpiredIntegrationEvent>
{
    public async Task Handle(EventSuspensionExpiredIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var staff = await userPublicApi.GetByIdAsync(integrationEvent.SuspendedBy, cancellationToken);
        if (staff is null || string.IsNullOrWhiteSpace(staff.Email))
        {
            logger.LogWarning("Staff not found/email empty. EventId: {EventId}", integrationEvent.EventId);
            return;
        }

        var subject = $"[AIPromo] Event quá hạn chỉnh sửa: {integrationEvent.EventTitle}";
        var body =
            $"Xin chào {staff.FullName},\n\n" +
            $"Event \"{integrationEvent.EventTitle}\" đã quá hạn chỉnh sửa ({integrationEvent.SuspendedUntilAtUtc:yyyy-MM-dd HH:mm:ss} UTC).\n" +
            "Vui lòng xử lý thủ công theo nghiệp vụ (ví dụ: cancel manual nếu cần).\n\n" +
            "AIPromo System";

        await emailSender.SendAsync(new EmailMessage(staff.Email, subject, body), cancellationToken);
    }
}
