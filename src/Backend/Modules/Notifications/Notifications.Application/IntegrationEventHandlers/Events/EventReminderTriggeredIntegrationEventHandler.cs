using Events.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Notifications;
using Ticketing.PublicApi;
using Users.PublicApi.PublicApi;

namespace Notifications.Application.IntegrationEventHandlers;

public sealed class EventReminderTriggeredIntegrationEventHandler(
    IUserPublicApi userPublicApi,
    ITicketingPublicApi ticketingPublicApi,
    IEmailSender emailSender,
    ILogger<EventReminderTriggeredIntegrationEventHandler> logger)
    : IIntegrationEventHandler<EventReminderTriggeredIntegrationEvent>
{

    public async Task Handle(
        EventReminderTriggeredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var buyerIds = await ticketingPublicApi.GetTicketBuyerIdsByEventIdAsync(integrationEvent.EventId, cancellationToken);
        if (buyerIds.Count == 0)
        {
            logger.LogInformation(
                "No ticket buyers found for event {EventId}, skipping reminder emails",
                integrationEvent.EventId);
            return;
        }

        var users = await userPublicApi.GetUserMapByIdsAsync(buyerIds, cancellationToken);

        var startTimeVn = integrationEvent.EventStartAtUtc.AddHours(7);
        var formattedTime = startTimeVn.ToString("HH:mm - dd/MM/yyyy");

        var subject = $"[AIPromo] Nhắc nhở - Sự kiện sắp diễn ra: {integrationEvent.EventTitle}";

        foreach (var user in users.Values)
        {
            var body = $"""
            <!DOCTYPE html>
            <html lang="vi">
            <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
            <body style="margin:0;padding:0;background:#0b0818;font-family:'Segoe UI',Arial,sans-serif;">

              <table width="100%" cellpadding="0" cellspacing="0" style="background:#0b0818;padding:32px 0;">
                <tr><td align="center">
                  <table width="600" cellpadding="0" cellspacing="0"
                         style="background:#13102a;border-radius:16px;border:1px solid #2a2250;overflow:hidden;max-width:600px;">

                    <tr>
                      <td style="background:linear-gradient(135deg,#7c3aed,#4f46e5);padding:32px;text-align:center;">
                        <div style="font-size:28px;font-weight:800;color:#ffffff;letter-spacing:-0.5px;">AIPromo</div>
                        <div style="font-size:14px;color:#ddd6fe;margin-top:6px;">Nhắc nhở sự kiện · 24 giờ còn lại</div>
                      </td>
                    </tr>

                    <tr>
                      <td style="padding:24px 32px 0;text-align:center;">
                        <span style="display:inline-block;background:#1e1a3a;border:1px solid #4f46e5;
                                     border-radius:999px;padding:6px 20px;font-size:13px;
                                     color:#a78bfa;font-weight:600;letter-spacing:0.5px;">
                          ⏰ &nbsp;Còn 24 giờ nữa
                        </span>
                      </td>
                    </tr>

                    <tr>
                      <td style="padding:20px 32px 0;">
                        <p style="margin:0;font-size:16px;color:#e2e8f0;">
                          Xin chào <strong style="color:#a78bfa;">{user.FullName}</strong>,
                        </p>
                        <p style="margin:12px 0 0;font-size:14px;color:#94a3b8;line-height:1.7;">
                          Sự kiện bạn đã mua vé sẽ bắt đầu trong vòng <strong style="color:#e2e8f0;">24 giờ tới</strong>.
                          Đừng bỏ lỡ sự kiện thú vị này!
                        </p>
                      </td>
                    </tr>

                    <tr>
                      <td style="padding:24px 32px;">
                        <table width="100%" cellpadding="0" cellspacing="0"
                               style="background:#1e1a3a;border-radius:12px;border:1px solid #2a2250;overflow:hidden;">
                          <tr>
                            <td style="padding:20px 24px;border-bottom:1px solid #2a2250;">
                              <span style="font-size:11px;text-transform:uppercase;letter-spacing:1px;color:#6b60a0;">Sự kiện</span><br>
                              <span style="font-size:16px;font-weight:700;color:#e2e8f0;margin-top:4px;display:block;">
                                {integrationEvent.EventTitle}
                              </span>
                            </td>
                          </tr>
                          <tr>
                            <td style="padding:16px 24px;">
                              <table width="100%" cellpadding="0" cellspacing="0">
                                <tr>
                                  <td style="width:50%;padding-right:12px;">
                                    <span style="font-size:11px;text-transform:uppercase;letter-spacing:1px;color:#6b60a0;">Thời gian bắt đầu</span><br>
                                    <span style="font-size:14px;font-weight:600;color:#c4b5fd;margin-top:4px;display:block;">
                                      🕐 &nbsp;{formattedTime} (GMT+7)
                                    </span>
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>

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
                new EmailMessage(user.Email ?? string.Empty, subject, body, IsHtml: true),
                cancellationToken);
        }

        logger.LogInformation(
            "Sent 24h reminder emails to {BuyerCount} ticket buyers for event {EventId}",
            users.Count,
            integrationEvent.EventId);
    }
}
