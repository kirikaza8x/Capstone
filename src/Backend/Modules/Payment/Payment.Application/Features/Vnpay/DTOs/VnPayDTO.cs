namespace Payment.Application.Features.VnPay.Dtos;

/// <summary>
/// Result of validating VNPay callback response
/// </summary>
public record PaymentResponseResult(
    bool IsValid,
    bool IsSuccess,
    string Message,
    string? ResponseCode,
    string? TransactionNo,
    decimal? Amount,
    string? OrderId
);


/// <summary>
/// Result of querying payment status from VNPay
/// </summary>
public record PaymentStatusQueryResult(
    bool IsSuccess,
    string Status,
    string Message,
    decimal? Amount,
    string? TransactionId,
    DateTime? TransactionDate
);

public class VnPayResultDto
{
    public Guid ItemId { get; set; }
    public bool PaymentSuccess { get; set; }
    public string? PaymentMessage { get; set; }
    public string? TransactionNo { get; set; }
    public string? ResponseCode { get; set; }
    public DateTime CheckedOutAt { get; set; }
}

public record InitiatePaymentResponseDto(
    string PaymentUrl
);
