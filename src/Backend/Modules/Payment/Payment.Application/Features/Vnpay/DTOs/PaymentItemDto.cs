namespace Payments.Application.DTOs.Payment;

// Used in both request (initiate) and response (history)
public record PaymentItemDto(
    Guid EventId,
    decimal Amount
);
