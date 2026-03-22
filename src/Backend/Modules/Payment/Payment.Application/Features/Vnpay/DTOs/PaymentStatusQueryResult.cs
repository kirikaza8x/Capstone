namespace Payments.Application.DTOs.VnPay;

public record PaymentStatusQueryResult(
    bool IsSuccess,
    string? StatusCode,
    string? Message,
    decimal? Amount,
    string? TransactionNo,
    DateTime? TransactionDateTime
);