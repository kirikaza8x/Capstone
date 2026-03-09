// using Application.Features.VnPay.DTOs;


using Payment.Application.Features.VnPay.Dtos;

public interface IVnPayService
{
    /// <summary>
    /// Creates a VNPay payment URL for checkout
    /// </summary>
    string CreatePaymentUrl(
        decimal amount, 
        string orderId, 
        string orderDescription, 
        string ipAddress, 
        string? customReturnUrl = null);

    /// <summary>
    /// Validates VNPay callback response from payment gateway
    /// </summary>
    PaymentResponseResult ValidateCallback(IDictionary<string, string> queryParams);

    /// <summary>
    /// Queries payment status directly from VNPay API
    /// </summary>
    Task<PaymentStatusQueryResult> QueryPaymentStatusAsync(string orderId, string transactionDate);
}