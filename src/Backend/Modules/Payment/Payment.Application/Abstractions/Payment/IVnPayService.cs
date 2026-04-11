using Payments.Application.DTOs.VnPay;

namespace Payments.Application.Abstractions
{
    public interface IVnPayService
    {
        /// <summary>
        /// Create a VNPay payment URL for redirecting the user.
        /// </summary>
        string CreatePaymentUrl(
            decimal amount,
            string txnRef,
            string orderDescription,
            string? ipAddress,
            string? customReturnUrl = null);

        /// <summary>
        /// Validate the callback from VNPay and return parsed result.
        /// </summary>
        PaymentCallbackResult ValidateCallback(IDictionary<string, string> queryParams);

        /// <summary>
        /// Query VNPay for the current status of a transaction.
        /// </summary>
        Task<PaymentStatusQueryResult> QueryPaymentStatusAsync(
            string orderId,
            string transactionDate,
            CancellationToken cancellationToken = default);
    }
}
