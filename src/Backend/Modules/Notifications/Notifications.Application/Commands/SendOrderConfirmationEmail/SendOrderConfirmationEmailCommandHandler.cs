using Microsoft.Extensions.Logging;
using Notifications.Domain.Errors;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Notifications;
using Shared.Domain.Abstractions;
using Users.PublicApi.PublicApi;

namespace Notifications.Application.Commands.SendOrderConfirmationEmail;

internal sealed class SendOrderConfirmationEmailCommandHandler(
    IUserPublicApi userPublicApi,
    IEmailSender emailSender,
    ILogger<SendOrderConfirmationEmailCommandHandler> logger)
    : ICommandHandler<SendOrderConfirmationEmailCommand>
{
    public async Task<Result> Handle(
        SendOrderConfirmationEmailCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userPublicApi.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            logger.LogWarning(
                "User not found or email empty. OrderId: {OrderId}",
                command.OrderId);
            return Result.Failure(NotificationErrors.Email.UserNotFound(command.UserId));
        }

        var ticketRows = string.Join("\n", command.Items.Select((item, index) =>
        {
            var qrUrl = $"https://quickchart.io/qr?text={Uri.EscapeDataString(item.QrCode)}&size=160&margin=2";
            return $"""
            <tr>
              <td style="padding:16px;border-bottom:1px solid #2a2250;text-align:center;width:60px;color:#a78bfa;font-size:13px;font-weight:600;">
                #{index + 1}
              </td>
              <td style="padding:16px;border-bottom:1px solid #2a2250;">
                <div style="font-size:12px;color:#94a3b8;font-family:monospace;word-break:break-all;">{item.QrCode}</div>
              </td>
              <td style="padding:16px;border-bottom:1px solid #2a2250;text-align:center;">
                <img src="{qrUrl}" width="100" height="100" alt="QR #{index + 1}"
                     style="border-radius:8px;border:2px solid #7c3aed;display:block;margin:0 auto;" />
              </td>
            </tr>
            """;
        }));

        var subject = $"[AIPromo] Xác nhận đặt vé – Đơn hàng #{command.OrderId:N}";

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
                            <div style="font-size:14px;color:#ddd6fe;margin-top:6px;">Xác nhận đặt vé thành công ✓</div>
                          </td>
                        </tr>

                        <!-- Greeting -->
                        <tr>
                          <td style="padding:28px 32px 0;">
                            <p style="margin:0;font-size:16px;color:#e2e8f0;">Xin chào <strong style="color:#a78bfa;">{user.FullName}</strong>,</p>
                            <p style="margin:10px 0 0;font-size:14px;color:#94a3b8;line-height:1.6;">
                              Đơn hàng của bạn đã được xác nhận thành công. Vui lòng xuất trình QR code bên dưới khi vào cổng sự kiện.
                            </p>
                          </td>
                        </tr>

                        <!-- Order info -->
                        <tr>
                          <td style="padding:24px 32px;">
                            <table width="100%" cellpadding="0" cellspacing="0"
                                   style="background:#1e1a3a;border-radius:12px;border:1px solid #2a2250;overflow:hidden;">
                              <tr>
                                <td style="padding:14px 20px;border-bottom:1px solid #2a2250;">
                                  <span style="font-size:11px;text-transform:uppercase;letter-spacing:1px;color:#6b60a0;">Mã đơn hàng</span><br>
                                  <span style="font-size:14px;font-family:monospace;color:#e2e8f0;font-weight:600;">{command.OrderId:N}</span>
                                </td>
                                <td style="padding:14px 20px;border-bottom:1px solid #2a2250;border-left:1px solid #2a2250;">
                                  <span style="font-size:11px;text-transform:uppercase;letter-spacing:1px;color:#6b60a0;">Tổng tiền</span><br>
                                  <span style="font-size:18px;color:#a78bfa;font-weight:800;">{command.TotalPrice:N0} ₫</span>
                                </td>
                              </tr>
                              <tr>
                                <td colspan="2" style="padding:14px 20px;">
                                  <span style="font-size:11px;text-transform:uppercase;letter-spacing:1px;color:#6b60a0;">Thời gian thanh toán</span><br>
                                  <span style="font-size:14px;color:#e2e8f0;">{command.PaidAtUtc:dd/MM/yyyy HH:mm:ss} UTC</span>
                                </td>
                              </tr>
                            </table>
                          </td>
                        </tr>

                        <!-- QR tickets -->
                        <tr>
                          <td style="padding:0 32px 28px;">
                            <div style="font-size:13px;font-weight:700;text-transform:uppercase;letter-spacing:1px;color:#7c3aed;margin-bottom:12px;">
                              🎫 Chi tiết vé ({command.Items.Count} vé)
                            </div>
                            <table width="100%" cellpadding="0" cellspacing="0"
                                   style="background:#1e1a3a;border-radius:12px;border:1px solid #2a2250;overflow:hidden;">
                              <thead>
                                <tr style="background:#16133a;">
                                  <th style="padding:12px 16px;font-size:11px;color:#6b60a0;text-align:center;font-weight:600;text-transform:uppercase;">#</th>
                                  <th style="padding:12px 16px;font-size:11px;color:#6b60a0;text-align:left;font-weight:600;text-transform:uppercase;">Mã vé</th>
                                  <th style="padding:12px 16px;font-size:11px;color:#6b60a0;text-align:center;font-weight:600;text-transform:uppercase;">QR Code</th>
                                </tr>
                              </thead>
                              <tbody>
                                {ticketRows}
                              </tbody>
                            </table>
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
            new EmailMessage(user.Email, subject, body),
            cancellationToken);

        return Result.Success();
    }
}
