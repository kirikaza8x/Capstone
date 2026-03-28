using Events.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Notifications;
using Users.PublicApi.PublicApi;

namespace Notifications.Application.IntegrationEventHandlers.Events;

internal sealed class EventMemberInvitedIntegrationEventHandler(
    IUserPublicApi userPublicApi,
    IEmailSender emailSender,
    ILogger<EventMemberInvitedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<EventMemberInvitedIntegrationEvent>
{
    public async Task Handle(EventMemberInvitedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var user = await userPublicApi.GetByIdAsync(integrationEvent.UserId, cancellationToken);

        if (user is null || string.IsNullOrWhiteSpace(integrationEvent.Email))
        {
            logger.LogWarning(
                "User not found or email is empty. UserId: {UserId}, EventId: {EventId}",
                integrationEvent.UserId,
                integrationEvent.EventId);
            return;
        }

        var subject = $"[AIPromo] Lời mời tham gia quản lý sự kiện: {integrationEvent.EventTitle}";

        var body =
            $"Xin chào {user.FullName},\n\n" +
            $"Bạn vừa nhận được lời mời tham gia vào đội ngũ quản lý cho sự kiện \"{integrationEvent.EventTitle}\".\n\n" +
            "Vui lòng đăng nhập vào hệ thống AIPromo để xem chi tiết và xác nhận lời mời này.\n\n" +
            "Trân trọng,\n" +
            "AIPromo";

        await emailSender.SendAsync(new EmailMessage(integrationEvent.Email, subject, body), cancellationToken);

        logger.LogInformation(
            "Successfully sent invitation notification to {Email} for Event {EventId}",
            integrationEvent.Email,
            integrationEvent.EventId);
    }
}
