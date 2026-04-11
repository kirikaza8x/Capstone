namespace Payments.Application.DTOs.VnPay;

public record PaymentCallbackResult(
    bool IsValid,
    bool IsSuccess,
    string? Message,
    string? ResponseCode,
    string? TransactionNo,
    decimal? Amount,
    string? OrderId
);
