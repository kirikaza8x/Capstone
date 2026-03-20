using Payment.Application.Features.VnPay.Dtos;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Commands.MassRefundByEvent;

public record MassRefundByEventCommand(Guid EventId) : ICommand<MassRefundResultDto>;


