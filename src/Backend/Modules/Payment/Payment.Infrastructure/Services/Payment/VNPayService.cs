using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payment.Infrastructure.Configs;
using Payments.Application.Abstractions;
using Payments.Application.DTOs.VnPay;

namespace Payments.Infrastructure.Services;

public class VnPayService : IVnPayService
{
    private readonly VnPayConfig _config;
    private readonly ILogger<VnPayService> _logger;

    public VnPayService(
        IOptions<VnPayConfig> options,
        ILogger<VnPayService> logger)
    {
        _config = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        if (!string.IsNullOrEmpty(_config.HashSecret))
            _config.HashSecret = _config.HashSecret.Trim();
    }

    public string CreatePaymentUrl(
        decimal amount,
        string txnRef,
        string orderDescription,
        string? ipAddress,
        string? customReturnUrl = null)
    {
        if (string.IsNullOrEmpty(_config.Url)
         || string.IsNullOrEmpty(_config.TmnCode)
         || string.IsNullOrEmpty(_config.HashSecret))
            throw new InvalidOperationException("VNPay configuration is incomplete.");

        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        if (string.IsNullOrWhiteSpace(txnRef))
            throw new ArgumentException("TxnRef is required.", nameof(txnRef));

        var returnUrl = string.IsNullOrEmpty(customReturnUrl) ? _config.ReturnUrl : customReturnUrl;
        var amountInt = (long)Math.Truncate(amount * 100M);
        var createDate = GetVietnamNow().ToString("yyyyMMddHHmmss");

        var query = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = _config.TmnCode,
            ["vnp_Amount"] = amountInt.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["vnp_CreateDate"] = createDate,
            ["vnp_CurrCode"] = "VND",
            ["vnp_IpAddr"] = NormalizeIp(ipAddress),
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

        var secureHash = HmacSha512(_config.HashSecret, signData);

        var urlPart = string.Join("&", sanitized.Select(kvp =>
            $"{kvp.Key}={Uri.EscapeDataString(kvp.Value.Trim())}"));

        return $"{_config.Url}?{urlPart}&vnp_SecureHashType=HMACSHA512&vnp_SecureHash={secureHash}";
    }

    public PaymentCallbackResult ValidateCallback(IDictionary<string, string> queryParams)
    {
        if (string.IsNullOrEmpty(_config.HashSecret))
            return Fail("VNPay secret missing.");

        if (!queryParams.TryGetValue("vnp_SecureHash", out var receivedHash)
         || string.IsNullOrEmpty(receivedHash))
        {
            _logger.LogWarning("VNPay callback missing secure hash.");
            return Fail("Missing secure hash.");
        }

        var copy = new SortedDictionary<string, string>(
            queryParams
                .Where(kvp => kvp.Key != "vnp_SecureHash"
                           && kvp.Key != "vnp_SecureHashType"
                           && !string.IsNullOrEmpty(kvp.Value))
                .ToDictionary(k => k.Key, v => v.Value),
            StringComparer.Ordinal);

        var signData = string.Join("&", copy.Select(kvp =>
     $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value.Trim())}"));
        var computed = HmacSha512(_config.HashSecret, signData);
        var isValid = string.Equals(computed, receivedHash, StringComparison.OrdinalIgnoreCase);

        copy.TryGetValue("vnp_ResponseCode", out var responseCode);
        copy.TryGetValue("vnp_TransactionNo", out var transactionNo);
        copy.TryGetValue("vnp_TxnRef", out var orderId);

        decimal? amount = null;
        if (copy.TryGetValue("vnp_Amount", out var amountStr)
         && long.TryParse(amountStr, out var amountVal))
            amount = amountVal / 100M;

        var isSuccess = isValid && responseCode == "00";

        _logger.LogInformation(
            "VNPay callback: IsValid={IsValid}, ResponseCode={Code}",
            isValid, responseCode);

        return new PaymentCallbackResult(
            isValid, isSuccess,
            GetMessage(responseCode, isValid),
            responseCode, transactionNo, amount, orderId);
    }

    public async Task<PaymentStatusQueryResult> QueryPaymentStatusAsync(
        string orderId,
        string transactionDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_config.TmnCode)
             || string.IsNullOrEmpty(_config.HashSecret))
                return new PaymentStatusQueryResult(
                    false, "CONFIG_ERROR", "VNPay config missing.", null, null, null);

            var requestId = Guid.NewGuid().ToString("N")[..16];
            var createDate = GetVietnamNow().ToString("yyyyMMddHHmmss");

            // querydr sign order is pipe-separated fixed field order — NOT sorted like pay URL
            var signData = string.Join("|", new[]
            {
                requestId,
                "2.1.0",
                "querydr",
                _config.TmnCode,
                orderId,
                transactionDate,
                createDate,
                "127.0.0.1",
                $"Query transaction {orderId}"
            });

            var secureHash = HmacSha512(_config.HashSecret, signData);

            var requestBody = new
            {
                vnp_RequestId = requestId,
                vnp_Version = "2.1.0",
                vnp_Command = "querydr",
                vnp_TmnCode = _config.TmnCode,
                vnp_TxnRef = orderId,
                vnp_OrderInfo = $"Query transaction {orderId}",
                vnp_TransactionDate = transactionDate,
                vnp_CreateDate = createDate,
                vnp_IpAddr = "127.0.0.1",
                vnp_SecureHash = secureHash
            };

            using var client = new HttpClient();
            using var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(_config.QueryDrUrl, content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            var data = ParseResponse(responseBody);

            if (data.TryGetValue("vnp_ResponseCode", out var code) && code == "00")
            {
                var isSuccess = data.TryGetValue("vnp_TransactionStatus", out var s) && s == "00";

                decimal? amount = null;
                if (data.TryGetValue("vnp_Amount", out var a)
                 && long.TryParse(a, out var av))
                    amount = av / 100M;

                DateTime? payDate = null;
                if (data.TryGetValue("vnp_PayDate", out var pd)
                 && DateTime.TryParseExact(pd, "yyyyMMddHHmmss", null,
                        System.Globalization.DateTimeStyles.None, out var pdt))
                    payDate = pdt;

                _logger.LogInformation(
                    "QueryDr success: OrderId={OrderId}, Status={Status}",
                    orderId, isSuccess ? "SUCCESS" : "FAILED");

                return new PaymentStatusQueryResult(
                    isSuccess,
                    isSuccess ? "SUCCESS" : "FAILED",
                    data.GetValueOrDefault("vnp_Message", "Query successful"),
                    amount,
                    data.GetValueOrDefault("vnp_TransactionNo"),
                    payDate);
            }

            _logger.LogWarning(
                "QueryDr failed: OrderId={OrderId}, Message={Message}",
                orderId, data.GetValueOrDefault("vnp_Message"));

            return new PaymentStatusQueryResult(
                false, "QUERY_FAILED",
                data.GetValueOrDefault("vnp_Message", "Query failed"),
                null, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QueryDr exception: OrderId={OrderId}", orderId);
            return new PaymentStatusQueryResult(
                false, "ERROR", ex.Message, null, null, null);
        }
    }

    // --------------------
    // Helpers
    // --------------------
    private static PaymentCallbackResult Fail(string message) =>
        new(false, false, message, null, null, null, null);

    private static string HmacSha512(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA512(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    private static Dictionary<string, string> ParseResponse(string response)
    {
        var result = new Dictionary<string, string>();
        try
        {
            var doc = System.Text.Json.JsonDocument.Parse(response);
            foreach (var prop in doc.RootElement.EnumerateObject())
                result[prop.Name] = prop.Value.GetString() ?? string.Empty;
        }
        catch (System.Text.Json.JsonException)
        {
            foreach (var pair in response.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = pair.Split('=', 2);
                if (kv.Length == 2)
                    result[kv[0]] = Uri.UnescapeDataString(kv[1]);
            }
        }
        return result;
    }

    private static string GetMessage(string? code, bool isValid)
    {
        if (!isValid) return "Invalid signature.";
        return code switch
        {
            "00" => "Transaction successful.",
            "07" => "Transaction suspected of fraud.",
            "09" => "Card not registered for internet banking.",
            "10" => "Card authentication failed.",
            "11" => "Transaction timeout.",
            "12" => "Card is locked.",
            "13" => "Invalid OTP.",
            "24" => "Transaction cancelled.",
            "51" => "Insufficient balance.",
            "65" => "Daily transaction limit exceeded.",
            "75" => "Bank under maintenance.",
            "79" => "Wrong payment password.",
            _ => $"Transaction failed. Code: {code}"
        };
    }

    private static string NormalizeIp(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return "127.0.0.1";
        if (ip is "::1" or "[::1]") return "127.0.0.1";

        ip = ip.Trim('[', ']');

        if (ip.Contains(':'))
            return ip.StartsWith("::ffff:", StringComparison.OrdinalIgnoreCase)
                ? ip[7..]
                : "127.0.0.1";

        return ip.Trim();
    }

    private static DateTime GetVietnamNow()
    {
        try
        {
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                return TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"));
            }
            catch (TimeZoneNotFoundException)
            {
                return DateTime.UtcNow.AddHours(7);
            }
        }
    }
}
