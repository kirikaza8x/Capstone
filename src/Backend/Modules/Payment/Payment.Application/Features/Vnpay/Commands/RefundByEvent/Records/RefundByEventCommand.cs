using Payment.Application.Features.VnPay.Dtos;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Commands.RefundByEvent;

public record RefundByEventCommand(
    Guid UserId,
    Guid EventId
) : ICommand<RefundByEventResultDto>;

