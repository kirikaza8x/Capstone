using Payments.Application.Features.Vnpay.DTOs;
using Shared.Application.Abstractions.Messaging;

public record GetRevenueByEventQuery(Guid EventId)
    : IQuery<EventRevenueDto>;