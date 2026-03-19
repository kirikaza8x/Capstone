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

        // Safeguard: trim secret stored in configuration
        if (!string.IsNullOrEmpty(_vnPay.HashSecret))
        {
            _vnPay.HashSecret = _vnPay.HashSecret.Trim();
        }
    }

    public string CreatePaymentUrl(decimal amount, string orderId, string orderDescription, string ipAddress, string? customReturnUrl = null)
    {
        var vnp_Url = _vnPay.Url;
        var vnp_TmnCode = _vnPay.TmnCode;
        var vnp_HashSecret = _vnPay.HashSecret;
        var vnp_ReturnUrl = string.IsNullOrEmpty(customReturnUrl) ? _vnPay.ReturnUrl : customReturnUrl;

        if (string.IsNullOrEmpty(vnp_Url) || string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret))
            throw new InvalidOperationException("VNPay configuration is missing.");
        if (amount <= 0) throw new ArgumentException("Amount must be greater than 0", nameof(amount));
        if (string.IsNullOrWhiteSpace(orderId)) throw new ArgumentException("OrderId is required", nameof(orderId));
        // VNPay expects amount in VND * 100 (integer)
        var amountInt = (long)Math.Truncate(amount * 1000000M);
        var createDate = GetVietnamNow().ToString("yyyyMMddHHmmss");
        var query = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = vnp_TmnCode,
            ["vnp_Amount"] = amountInt.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["vnp_CreateDate"] = createDate,
            ["vnp_CurrCode"] = "VND",
            ["vnp_IpAddr"] = NormalizeIpAddress(ipAddress),
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = orderId,
            ["vnp_OrderType"] = "other",
            ["vnp_ReturnUrl"] =  "https://pyridic-ambrose-overwarmed.ngrok-free.dev/checkout.html", //vnp_ReturnUrl?.Trim() ?? "",
            ["vnp_TxnRef"] = Guid.NewGuid().ToString()
        };
        var sanitized = new SortedDictionary<string, string>(query
            .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
            .ToDictionary(k => k.Key, v => v.Value), StringComparer.Ordinal);

        foreach (var kvp in sanitized)
        {
            var preview = kvp.Value.Length <= 200 ? kvp.Value : kvp.Value.Substring(0, 200) + "...(truncated)";
        }
        var signData = string.Join("&", sanitized.Select(kvp =>
            $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value.Trim())}"));
        var secureHash = HmacSHA512(vnp_HashSecret.Trim(), signData);
        var signPreview = signData.Length <= 400 ? signData : signData.Substring(0, 400) + "...(truncated)";
        var urlEncoded = string.Join("&", sanitized.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value.Trim())}"));
        var fullUrl = $"{vnp_Url}?{urlEncoded}&vnp_SecureHashType=HMACSHA512&vnp_SecureHash={secureHash}";
        var encodedPreview = fullUrl.Length <= 800 ? fullUrl : fullUrl.Substring(0, 800) + "...(truncated)";


        return fullUrl;
    }

    public PaymentResponseResult ValidateCallback(IDictionary<string, string> queryParams)
    {
        var vnp_HashSecret = _vnPay.HashSecret;

        if (string.IsNullOrEmpty(vnp_HashSecret))
        {
            return new PaymentResponseResult(false, false, "VNPay secret missing", null, null, null, null);
        }

        if (!queryParams.TryGetValue("vnp_SecureHash", out var receivedHash) || string.IsNullOrEmpty(receivedHash))
        {
            _logger.LogWarning("VNPay callback missing secure hash");
            return new PaymentResponseResult(false, false, "Missing secure hash", null, null, null, null);
        }

        var copy = new SortedDictionary<string, string>(queryParams
            .Where(kvp => kvp.Key != "vnp_SecureHash" && kvp.Key != "vnp_SecureHashType" && !string.IsNullOrEmpty(kvp.Value))
            .ToDictionary(k => k.Key, v => v.Value), StringComparer.Ordinal);

        var signData = string.Join("&", copy.Select(kvp => $"{kvp.Key}={kvp.Value.Trim()}"));
        var computedHash = HmacSHA512(vnp_HashSecret, signData);
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
            var vnp_TmnCode = _vnPay.TmnCode;
            var vnp_HashSecret = _vnPay.HashSecret;
            var queryUrl = _vnPay.ReturnUrl ?? "https://sandbox.vnpayment.vn/querydr/PaymentVerify.aspx";

            if (string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret))
            {
                return new PaymentStatusQueryResult(false, "CONFIG_ERROR", "VNPay configuration is missing", null, null, null);
            }

            var requestData = new Dictionary<string, string>
            {
                ["vnp_RequestId"] = Guid.NewGuid().ToString(),
                ["vnp_Version"] = "2.1.0",
                ["vnp_Command"] = "querydr",
                ["vnp_TmnCode"] = vnp_TmnCode,
                ["vnp_TxnRef"] = orderId,
                ["vnp_OrderInfo"] = $"Query payment status for order {orderId}",
                ["vnp_TransactionNo"] = "",
                ["vnp_TransactionDate"] = transactionDate,
                ["vnp_CreateDate"] = GetVietnamNow().ToString("yyyyMMddHHmmss"),
                ["vnp_IpAddr"] = "127.0.0.1"
            };

            var sortedParams = requestData.OrderBy(x => x.Key).ToList();
            var queryString = string.Join("&", sortedParams.Select(x => $"{x.Key}={x.Value}"));
            var signature = HmacSHA512(vnp_HashSecret, queryString);
            requestData["vnp_SecureHash"] = signature;

            using var httpClient = new HttpClient();
            var formData = new FormUrlEncodedContent(requestData);

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
                    DateTime.TryParseExact(payDateStr, "yyyyMMddHHmmss", null,
                        System.Globalization.DateTimeStyles.None, out var payDate))
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

            _logger.LogWarning("Failed to query payment status for order {OrderId}: {Message}",
                orderId, responseData.GetValueOrDefault("vnp_Message"));

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
            var tzi = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);
        }
        catch
        {
            try
            {
                var tzi = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);
            }
            catch
            {
                return DateTime.UtcNow.AddHours(7);
            }
        }
    }

    private static string HmacSHA512(string key, string data)
    {
        var hash = new StringBuilder();
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(data);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            byte[] hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
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
                result[property.Name] = property.Value.GetString() ?? "";
            }
        }
        catch
        {
            var pairs = response.Split('&');
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

        // Convert IPv6 localhost to IPv4
        if (ipAddress == "::1" || ipAddress == "[::1]")
            return "127.0.0.1";

        // Remove IPv6 brackets if present
        ipAddress = ipAddress.Trim('[', ']');

        // If it's IPv6, try to extract IPv4 if mapped
        if (ipAddress.Contains(':'))
        {
            // Check for IPv4-mapped IPv6 (::ffff:192.168.1.1)
            if (ipAddress.StartsWith("::ffff:", StringComparison.OrdinalIgnoreCase))
            {
                return ipAddress.Substring(7);
            }
            // For other IPv6, return default
            return "127.0.0.1";
        }

        return ipAddress.Trim();
    }
}
