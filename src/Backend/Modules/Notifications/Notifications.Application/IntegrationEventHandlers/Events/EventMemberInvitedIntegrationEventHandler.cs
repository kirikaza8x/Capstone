using Events.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Notifications;
using Users.PublicApi.PublicApi;

namespace Notifications.Application.IntegrationEventHandlers.Events;

public sealed class EventMemberInvitedIntegrationEventHandler(
    IUserPublicApi userPublicApi,
    IEmailSender emailSender,
    ILogger<EventMemberInvitedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<EventMemberInvitedIntegrationEvent>
{
    private const string FrontendBaseUrl = "http://aipromo.online";

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

        var confirmUrl = $"{FrontendBaseUrl}/verify-member?eventId={integrationEvent.EventId}&memberId={integrationEvent.UserId}";
        var subject = $"[AIPromo] Lời mời tham gia quản lý sự kiện: {integrationEvent.EventTitle}";

        var body = $"""
        <!DOCTYPE html>
        <html lang="vi">
        <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
        <body style="margin:0;padding:0;background:#0b0818;font-family:'Segoe UI',Arial,sans-serif;">

          <table width="100%" cellpadding="0" cellspacing="0" style="background:#0b0818;padding:32px 0;">
            <tr><td align="center">
              <table width="600" cellpadding="0" cellspacing="0"
                     style="background:#13102a;border-radius:16px;border:1px solid #2a2250;overflow:hidden;max-width:600px;">

                <!-- Header -->
                <tr>
                  <td style="background:linear-gradient(135deg,#7c3aed,#4f46e5);padding:32px;text-align:center;">
                    <div style="font-size:28px;font-weight:800;color:#ffffff;letter-spacing:-0.5px;">AIPromo</div>
                    <div style="font-size:14px;color:#ddd6fe;margin-top:6px;">Lời mời tham gia quản lý sự kiện</div>
                  </td>
                </tr>

                <!-- Greeting -->
                <tr>
                  <td style="padding:28px 32px 0;">
                    <p style="margin:0;font-size:16px;color:#e2e8f0;">
                      Xin chào <strong style="color:#a78bfa;">{user.FullName}</strong>,
                    </p>
                    <p style="margin:12px 0 0;font-size:14px;color:#94a3b8;line-height:1.7;">
                      Bạn vừa nhận được lời mời tham gia vào đội ngũ quản lý cho sự kiện bên dưới.
                      Vui lòng xác nhận để bắt đầu cộng tác.
                    </p>
                  </td>
                </tr>

                <!-- Event info -->
                <tr>
                  <td style="padding:24px 32px;">
                    <table width="100%" cellpadding="0" cellspacing="0"
                           style="background:#1e1a3a;border-radius:12px;border:1px solid #2a2250;overflow:hidden;">
                      <tr>
                        <td style="padding:20px 24px;">
                          <span style="font-size:11px;text-transform:uppercase;letter-spacing:1px;color:#6b60a0;">Sự kiện</span><br>
                          <span style="font-size:16px;font-weight:700;color:#e2e8f0;margin-top:4px;display:block;">
                            {integrationEvent.EventTitle}
                          </span>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>

                <!-- CTA Button -->
                <tr>
                  <td style="padding:0 32px 32px;text-align:center;">
                    <a href="{confirmUrl}"
                       style="display:inline-block;padding:14px 36px;background:linear-gradient(135deg,#7c3aed,#4f46e5);
                              color:#ffffff;font-size:15px;font-weight:700;text-decoration:none;
                              border-radius:12px;letter-spacing:0.3px;">
                      ✓ Xác nhận tham gia
                    </a>
                    <p style="margin:16px 0 0;font-size:12px;color:#4a4570;">
                      Hoặc copy link sau vào trình duyệt:<br>
                      <span style="color:#7c3aed;font-family:monospace;word-break:break-all;font-size:11px;">
                        {confirmUrl}
                      </span>
                    </p>
                  </td>
                </tr>

                <!-- Footer -->
                <tr>
                  <td style="background:#0d0a1f;padding:20px 32px;text-align:center;border-top:1px solid #2a2250;">
                    <p style="margin:0;font-size:12px;color:#4a4570;">
                      Email này được gửi tự động bởi hệ thống AIPromo.<br>
                      Vui lòng không trả lời email này.
                    </p>
                  </td>
                </tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;

        await emailSender.SendAsync(
            new EmailMessage(integrationEvent.Email, subject, body, IsHtml: true),
            cancellationToken);

        logger.LogInformation(
            "Successfully sent invitation email to {Email} for Event {EventId}",
            integrationEvent.Email,
            integrationEvent.EventId);
    }
}
