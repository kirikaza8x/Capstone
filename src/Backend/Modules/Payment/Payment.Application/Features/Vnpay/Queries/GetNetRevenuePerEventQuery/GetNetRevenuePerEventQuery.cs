using Payment.Application.Features.Vnpay.DTOs;
using Shared.Application.Abstractions.Messaging;

public record GetNetRevenuePerEventQuery : IQuery<IReadOnlyList<EventRevenueDto>>;