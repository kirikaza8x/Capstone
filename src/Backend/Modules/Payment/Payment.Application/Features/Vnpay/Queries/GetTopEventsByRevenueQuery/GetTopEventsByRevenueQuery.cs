using Payment.Application.Features.Vnpay.DTOs;
using Shared.Application.Abstractions.Messaging;

public record GetTopEventsByRevenueQuery(int TopN, bool ByNet = false)
    : IQuery<IReadOnlyList<EventRevenueDto>>;