using Payments.Application.Features.Vnpay.DTOs;
using Shared.Application.Abstractions.Messaging;

public record GetNetRevenueByEventQuery(Guid EventId)
    : IQuery<EventRevenueDto>;