namespace Payments.Application.Features.Vnpay.DTOs;

public record EventRevenueDto(
    Guid EventId,
    decimal Revenue
);