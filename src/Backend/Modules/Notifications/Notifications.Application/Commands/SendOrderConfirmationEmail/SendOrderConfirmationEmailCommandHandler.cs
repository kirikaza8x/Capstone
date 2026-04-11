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

        var ticketLines = string.Join("\n", command.Items.Select((item, index) =>
            $"  {index + 1}. QR: {item.QrCode}"));

        var subject = $"[AIPromo] Xác nhận đặt vé - Đơn hàng #{command.OrderId:N}";
        var body =
            $"Xin chào {user.FullName},\n\n" +
            $"Đơn hàng của bạn đã được xác nhận thành công.\n\n" +
            $"Mã đơn hàng: {command.OrderId:N}\n" +
            $"Tổng tiền: {command.TotalPrice:N0} VNĐ\n" +
            $"Thời gian thanh toán: {command.PaidAtUtc:yyyy-MM-dd HH:mm:ss} UTC\n\n" +
            $"Chi tiết vé:\n{ticketLines}\n\n" +
            "Vui lòng xuất trình QR code khi vào cổng.\n\n" +
            "AIPromo";

        await emailSender.SendAsync(
            new EmailMessage(user.Email, subject, body),
            cancellationToken);

        return Result.Success();
    }
}
