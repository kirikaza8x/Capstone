using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payment.Application.Features.VnPay.Dtos;
using Payment.Infrastructure.Configs;
using Payments.Application.Abstractions;

namespace Infrastructure.Payments;

/// <summary>
/// VNPay payment service - handles payment URL creation and response validation
/// </summary>
public class VnPayService : IVnPayService
{
    private readonly VnPayConfig _vnPay;
    private readonly ILogger<VnPayService> _logger;

    public VnPayService(IOptions<VnPayConfig> options, ILogger<VnPayService> logger)
    {
        _vnPay = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!string.IsNullOrEmpty(_vnPay.HashSecret))
        {
            _vnPay.HashSecret = _vnPay.HashSecret.Trim();
        }
    }

    public string CreatePaymentUrl(
    decimal amount,
    string txnRef,
    string orderDescription,
    string ipAddress,
    string? customReturnUrl = null)
    {
        var returnUrl = string.IsNullOrEmpty(customReturnUrl) ? _vnPay.ReturnUrl : customReturnUrl;

        if (string.IsNullOrEmpty(_vnPay.Url) || string.IsNullOrEmpty(_vnPay.TmnCode) || string.IsNullOrEmpty(_vnPay.HashSecret))
            throw new InvalidOperationException("VNPay configuration is missing.");

        if (amount <= 0) throw new ArgumentException("Amount must be greater than 0", nameof(amount));
        if (string.IsNullOrWhiteSpace(txnRef)) throw new ArgumentException("txnRef is required", nameof(txnRef));

        // VNPay expects amount in VND * 100
        var amountInt = (long)Math.Truncate(amount * 100M);
        var createDate = GetVietnamNow().ToString("yyyyMMddHHmmss");

        var query = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = _vnPay.TmnCode,
            ["vnp_Amount"] = amountInt.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["vnp_CreateDate"] = createDate,
            ["vnp_CurrCode"] = "VND",
            ["vnp_IpAddr"] = NormalizeIpAddress(ipAddress),
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = orderDescription,
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] = returnUrl?.Trim() ?? string.Empty,
            ["vnp_TxnRef"] = txnRef
        };

        var sanitized = new SortedDictionary<string, string>(
            query.Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                 .ToDictionary(k => k.Key, v => v.Value),
            StringComparer.Ordinal);

        var signData = string.Join("&", sanitized.Select(kvp =>
            $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value.Trim())}"));

        var secureHash = HmacSHA512(_vnPay.HashSecret, signData);

        var urlEncoded = string.Join("&", sanitized.Select(kvp =>
            $"{kvp.Key}={Uri.EscapeDataString(kvp.Value.Trim())}"));

        return $"{_vnPay.Url}?{urlEncoded}&vnp_SecureHashType=HMACSHA512&vnp_SecureHash={secureHash}";
    }

    public PaymentResponseResult ValidateCallback(IDictionary<string, string> queryParams)
    {
        if (string.IsNullOrEmpty(_vnPay.HashSecret))
        {
            return new PaymentResponseResult(false, false, "VNPay secret missing", null, null, null, null);
        }

        if (!queryParams.TryGetValue("vnp_SecureHash", out var receivedHash) || string.IsNullOrEmpty(receivedHash))
        {
            _logger.LogWarning("VNPay callback missing secure hash");
            return new PaymentResponseResult(false, false, "Missing secure hash", null, null, null, null);
        }

        var copy = new SortedDictionary<string, string>(
            queryParams.Where(kvp => kvp.Key != "vnp_SecureHash" && kvp.Key != "vnp_SecureHashType" && !string.IsNullOrEmpty(kvp.Value))
                       .ToDictionary(k => k.Key, v => v.Value),
            StringComparer.Ordinal);

        var signData = string.Join("&", copy.Select(kvp => $"{kvp.Key}={kvp.Value.Trim()}"));
        var computedHash = HmacSHA512(_vnPay.HashSecret, signData);
        var isValid = string.Equals(computedHash, receivedHash, StringComparison.OrdinalIgnoreCase);

        copy.TryGetValue("vnp_ResponseCode", out var responseCode);
        copy.TryGetValue("vnp_TransactionNo", out var transactionNo);
        copy.TryGetValue("vnp_TxnRef", out var orderId);

        decimal? amount = null;
        if (copy.TryGetValue("vnp_Amount", out var amountStr) && long.TryParse(amountStr, out var amountValue))
        {
            amount = amountValue / 100M;
        }

        var isSuccess = isValid && responseCode == "00";

        _logger.LogInformation("VNPay response validation: IsValid={IsValid}, ResponseCode={ResponseCode}", isValid, responseCode);

        return new PaymentResponseResult(
            isValid,
            isSuccess,
            GetResponseMessage(responseCode, isValid),
            responseCode,
            transactionNo,
            amount,
            orderId
        );
    }

    public async Task<PaymentStatusQueryResult> QueryPaymentStatusAsync(string orderId, string transactionDate)
    {
        try
        {
            var queryUrl = _vnPay.ReturnUrl ?? "https://sandbox.vnpayment.vn/querydr/PaymentVerify.aspx";

            if (string.IsNullOrEmpty(_vnPay.TmnCode) || string.IsNullOrEmpty(_vnPay.HashSecret))
            {
                return new PaymentStatusQueryResult(false, "CONFIG_ERROR", "VNPay configuration is missing", null, null, null);
            }

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
                ["vnp_CreateDate"] = GetVietnamNow().ToString("yyyyMMddHHmmss"),
                ["vnp_IpAddr"] = "127.0.0.1"
            };

            var sortedParams = requestData.OrderBy(x => x.Key).ToList();
            var queryString = string.Join("&", sortedParams.Select(x => $"{x.Key}={x.Value}"));

            requestData["vnp_SecureHash"] = HmacSHA512(_vnPay.HashSecret, queryString);

            using var httpClient = new HttpClient();
            using var formData = new FormUrlEncodedContent(requestData);

            var response = await httpClient.PostAsync(queryUrl, formData);
            var responseContent = await response.Content.ReadAsStringAsync();

            var responseData = ParseVnPayResponse(responseContent);

            if (responseData.TryGetValue("vnp_ResponseCode", out var respCode) && respCode == "00")
            {
                var isSuccess = responseData.TryGetValue("vnp_TransactionStatus", out var txnStatus) && txnStatus == "00";

                decimal? amount = null;
                if (responseData.TryGetValue("vnp_Amount", out var amountStr) && decimal.TryParse(amountStr, out var amountValue))
                {
                    amount = amountValue / 100M;
                }

                DateTime? transactionDateTime = null;
                if (responseData.TryGetValue("vnp_PayDate", out var payDateStr) &&
                    DateTime.TryParseExact(payDateStr, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var payDate))
                {
                    transactionDateTime = payDate;
                }

                _logger.LogInformation("Payment status queried successfully for order {OrderId}: {Status}", orderId, isSuccess ? "SUCCESS" : "FAILED");

                return new PaymentStatusQueryResult(
                    isSuccess,
                    isSuccess ? "SUCCESS" : "FAILED",
                    responseData.GetValueOrDefault("vnp_Message", "Payment status queried successfully"),
                    amount,
                    responseData.GetValueOrDefault("vnp_TransactionNo"),
                    transactionDateTime
                );
            }

            _logger.LogWarning("Failed to query payment status for order {OrderId}: {Message}", orderId, responseData.GetValueOrDefault("vnp_Message"));

            return new PaymentStatusQueryResult(
                false,
                "QUERY_FAILED",
                responseData.GetValueOrDefault("vnp_Message", "Failed to query payment status"),
                null,
                null,
                null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying payment status for order {OrderId}", orderId);
            return new PaymentStatusQueryResult(false, "ERROR", $"Error querying payment status: {ex.Message}", null, null, null);
        }
    }

    private static DateTime GetVietnamNow()
    {
        try
        {
            // Windows TimeZone
            var tzi = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                // Linux/macOS TimeZone
                var tzi = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);
            }
            catch (TimeZoneNotFoundException)
            {
                // Hard fallback
                return DateTime.UtcNow.AddHours(7);
            }
        }
    }

    private static string HmacSHA512(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA512(keyBytes);
        var hashValue = hmac.ComputeHash(inputBytes);

        var hash = new StringBuilder(hashValue.Length * 2);
        foreach (var b in hashValue)
        {
            hash.Append(b.ToString("x2"));
        }

        return hash.ToString();
    }

    private static Dictionary<string, string> ParseVnPayResponse(string response)
    {
        var result = new Dictionary<string, string>();

        try
        {
            var jsonDoc = System.Text.Json.JsonDocument.Parse(response);
            foreach (var property in jsonDoc.RootElement.EnumerateObject())
            {
                result[property.Name] = property.Value.GetString() ?? string.Empty;
            }
        }
        catch (System.Text.Json.JsonException)
        {
            // Fallback for non-JSON string
            var pairs = response.Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=', 2);
                if (keyValue.Length == 2)
                {
                    result[keyValue[0]] = Uri.UnescapeDataString(keyValue[1]);
                }
            }
        }

        return result;
    }

    private static string GetResponseMessage(string? responseCode, bool isHashValid)
    {
        if (!isHashValid) return "Invalid signature - payment verification failed";

        return responseCode switch
        {
            "00" => "Transaction successful",
            "07" => "Transaction suspected of fraud",
            "09" => "Card not registered for Internet Banking",
            "10" => "Card authentication failed",
            "11" => "Transaction timeout",
            "12" => "Card is locked",
            "13" => "Invalid OTP",
            "24" => "Transaction cancelled",
            "51" => "Insufficient balance",
            "65" => "Account exceeded daily transaction limit",
            "75" => "Payment bank is under maintenance",
            "79" => "Transaction exceeded limit",
            _ => $"Transaction failed with code: {responseCode}"
        };
    }

    private static string NormalizeIpAddress(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return "127.0.0.1";

        if (ipAddress == "::1" || ipAddress == "[::1]")
            return "127.0.0.1";

        ipAddress = ipAddress.Trim('[', ']');

        if (ipAddress.Contains(':'))
        {
            if (ipAddress.StartsWith("::ffff:", StringComparison.OrdinalIgnoreCase))
            {
                return ipAddress.Substring(7);
            }
            return "127.0.0.1";
        }

        return ipAddress.Trim();
    }
}
