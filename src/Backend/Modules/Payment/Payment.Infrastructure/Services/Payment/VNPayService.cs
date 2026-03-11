using System.Net;
using Payment.Application.Features.VnPay.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payment.Infrastructure.Configs;

namespace Infrastructure.Payments;

public class VnPayService : IVnPayService
{
    private readonly VnPayConfig _vnPay;
    private readonly ILogger<VnPayService> _logger;

    public VnPayService(IOptions<VnPayConfig> options, ILogger<VnPayService> logger)
    {
        _vnPay = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!string.IsNullOrEmpty(_vnPay.HashSecret))
            _vnPay.HashSecret = _vnPay.HashSecret.Trim();
    }

    public string CreatePaymentUrl(decimal amount, string orderId, string orderDescription, string ipAddress, string? customReturnUrl = null)
    {
        if (string.IsNullOrEmpty(_vnPay.Url) || string.IsNullOrEmpty(_vnPay.TmnCode) || string.IsNullOrEmpty(_vnPay.HashSecret))
            throw new InvalidOperationException("VNPay configuration is missing.");
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than 0", nameof(amount));
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentException("OrderId is required", nameof(orderId));

        // VNPay expects amount in VND * 100 (integer)
        var amountInt = (long)Math.Truncate(amount * 1000000M);

        var query = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = _vnPay.TmnCode,
            ["vnp_Amount"] = amountInt.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["vnp_CreateDate"] = VnPayHelpers.GetVietnamNow().ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = "VND",
            ["vnp_IpAddr"] = VnPayHelpers.NormalizeIpAddress(ipAddress),
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = orderId,
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] = _vnPay.ReturnUrl,
            ["vnp_TxnRef"] = Guid.NewGuid().ToString()
        };

        var sanitized = new SortedDictionary<string, string>(
            query.Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                 .ToDictionary(k => k.Key, v => v.Value),
            StringComparer.Ordinal);

        var signData = string.Join("&", sanitized.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value.Trim())}"));
        var secureHash = VnPayHelpers.HmacSHA512(_vnPay.HashSecret.Trim(), signData);
        var urlEncoded = string.Join("&", sanitized.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value.Trim())}"));

        return $"{_vnPay.Url}?{urlEncoded}&vnp_SecureHashType=HMACSHA512&vnp_SecureHash={secureHash}";
    }

    public PaymentResponseResult ValidateCallback(IDictionary<string, string> queryParams)
    {
        if (string.IsNullOrEmpty(_vnPay.HashSecret))
            return Fail("VNPay secret missing");

        if (!queryParams.TryGetValue("vnp_SecureHash", out var receivedHash) || string.IsNullOrEmpty(receivedHash))
        {
            _logger.LogWarning("VNPay callback missing secure hash");
            return Fail("Missing secure hash");
        }

        var copy = new SortedDictionary<string, string>(
            queryParams
                .Where(kvp => kvp.Key != "vnp_SecureHash" && kvp.Key != "vnp_SecureHashType" && !string.IsNullOrEmpty(kvp.Value))
                .ToDictionary(k => k.Key, v => v.Value),
            StringComparer.Ordinal);

        var signData = string.Join("&", copy.Select(kvp => $"{kvp.Key}={kvp.Value.Trim()}"));
        var isValid = string.Equals(VnPayHelpers.HmacSHA512(_vnPay.HashSecret, signData), receivedHash, StringComparison.OrdinalIgnoreCase);

        copy.TryGetValue("vnp_ResponseCode", out var responseCode);
        copy.TryGetValue("vnp_TransactionNo", out var transactionNo);
        copy.TryGetValue("vnp_TxnRef", out var txnOrderId);

        decimal? amount = null;
        if (copy.TryGetValue("vnp_Amount", out var amountStr) && long.TryParse(amountStr, out var amountValue))
            amount = amountValue / 100M;

        var isSuccess = isValid && responseCode == "00";

        _logger.LogInformation("VNPay response validation: IsValid={IsValid}, ResponseCode={ResponseCode}", isValid, responseCode);

        return new PaymentResponseResult(isValid, isSuccess, VnPayHelpers.GetResponseMessage(responseCode, isValid),
            responseCode, transactionNo, amount, txnOrderId);

        static PaymentResponseResult Fail(string message) => new(false, false, message, null, null, null, null);
    }

    public async Task<PaymentStatusQueryResult> QueryPaymentStatusAsync(string orderId, string transactionDate)
    {
        try
        {
            if (string.IsNullOrEmpty(_vnPay.TmnCode) || string.IsNullOrEmpty(_vnPay.HashSecret))
                return Fail("CONFIG_ERROR", "VNPay configuration is missing");

            var queryUrl = _vnPay.ReturnUrl ?? "https://sandbox.vnpayment.vn/querydr/PaymentVerify.aspx";

            var requestData = new Dictionary<string, string>
            {
                ["vnp_RequestId"] = Guid.NewGuid().ToString(),
                ["vnp_Version"] = "2.1.0",
                ["vnp_Command"] = "querydr",
                ["vnp_TmnCode"] = _vnPay.TmnCode,
                ["vnp_TxnRef"] = orderId,
                ["vnp_OrderInfo"] = $"Query payment status for order {orderId}",
                ["vnp_TransactionNo"] = "",
                ["vnp_TransactionDate"] = transactionDate,
                ["vnp_CreateDate"] = VnPayHelpers.GetVietnamNow().ToString("yyyyMMddHHmmss"),
                ["vnp_IpAddr"] = "127.0.0.1"
            };

            var queryString = string.Join("&", requestData.OrderBy(x => x.Key).Select(x => $"{x.Key}={x.Value}"));
            requestData["vnp_SecureHash"] = VnPayHelpers.HmacSHA512(_vnPay.HashSecret, queryString);

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(queryUrl, new FormUrlEncodedContent(requestData));
            var responseData = VnPayHelpers.ParseVnPayResponse(await response.Content.ReadAsStringAsync());

            if (!responseData.TryGetValue("vnp_ResponseCode", out var respCode) || respCode != "00")
            {
                _logger.LogWarning("Failed to query payment status for order {OrderId}: {Message}",
                    orderId, responseData.GetValueOrDefault("vnp_Message"));
                return Fail("QUERY_FAILED", responseData.GetValueOrDefault("vnp_Message", "Failed to query payment status"));
            }

            var isSuccess = responseData.TryGetValue("vnp_TransactionStatus", out var txnStatus) && txnStatus == "00";

            decimal? amount = null;
            if (responseData.TryGetValue("vnp_Amount", out var amountStr) && decimal.TryParse(amountStr, out var amountValue))
                amount = amountValue / 100M;

            DateTime? transactionDateTime = null;
            if (responseData.TryGetValue("vnp_PayDate", out var payDateStr) &&
                DateTime.TryParseExact(payDateStr, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var payDate))
                transactionDateTime = payDate;

            _logger.LogInformation("Payment status queried successfully for order {OrderId}: {Status}",
                orderId, isSuccess ? "SUCCESS" : "FAILED");

            return new PaymentStatusQueryResult(
                isSuccess,
                isSuccess ? "SUCCESS" : "FAILED",
                responseData.GetValueOrDefault("vnp_Message", "Payment status queried successfully"),
                amount,
                responseData.GetValueOrDefault("vnp_TransactionNo"),
                transactionDateTime
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying payment status for order {OrderId}", orderId);
            return Fail("ERROR", $"Error querying payment status: {ex.Message}");
        }

        static PaymentStatusQueryResult Fail(string status, string message) => new(false, status, message, null, null, null);
    }
}