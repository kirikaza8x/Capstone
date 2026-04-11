using Shared.Application.Abstractions.Messaging;
using Ticketing.IntegrationEvents;

namespace Notifications.Application.Commands.SendOrderConfirmationEmail;


public sealed record SendOrderConfirmationEmailCommand(
    Guid UserId,
    Guid OrderId,
    decimal TotalPrice,
    DateTime PaidAtUtc,
    IReadOnlyList<OrderConfirmedTicketItem> Items) : ICommand;
