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
    public Guid ItemId { get; set; }                // ID of the cart
    public bool PaymentSuccess { get; set; }        // Indicates if payment was successful
    public string? PaymentMessage { get; set; }      // Message to display to user
    public string? TransactionNo { get; set; }       // VNPay transaction number
    public string? ResponseCode { get; set; }        // VNPay response code
    public DateTime CheckedOutAt { get; set; }      // Timestamp of checkout
}