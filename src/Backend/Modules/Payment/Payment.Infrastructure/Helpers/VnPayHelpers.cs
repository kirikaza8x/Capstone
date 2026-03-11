using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Payments;

internal static class VnPayHelpers
{
    internal static string HmacSHA512(string key, string data)
    {
        var hash = new StringBuilder();
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        foreach (var b in hmac.ComputeHash(Encoding.UTF8.GetBytes(data)))
            hash.Append(b.ToString("x2"));
        return hash.ToString();
    }

    internal static DateTime GetVietnamNow()
    {
        try
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        }
        catch
        {
            try
            {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"));
            }
            catch
            {
                return DateTime.UtcNow.AddHours(7);
            }
        }
    }

    internal static string NormalizeIpAddress(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return "127.0.0.1";

        if (ipAddress == "::1" || ipAddress == "[::1]")
            return "127.0.0.1";

        ipAddress = ipAddress.Trim('[', ']');

        if (ipAddress.Contains(':'))
        {
            if (ipAddress.StartsWith("::ffff:", StringComparison.OrdinalIgnoreCase))
                return ipAddress.Substring(7);
            return "127.0.0.1";
        }

        return ipAddress.Trim();
    }

    internal static Dictionary<string, string> ParseVnPayResponse(string response)
    {
        var result = new Dictionary<string, string>();
        try
        {
            var jsonDoc = System.Text.Json.JsonDocument.Parse(response);
            foreach (var property in jsonDoc.RootElement.EnumerateObject())
                result[property.Name] = property.Value.GetString() ?? "";
        }
        catch
        {
            foreach (var pair in response.Split('&'))
            {
                var keyValue = pair.Split('=', 2);
                if (keyValue.Length == 2)
                    result[keyValue[0]] = Uri.UnescapeDataString(keyValue[1]);
            }
        }
        return result;
    }

    internal static string GetResponseMessage(string? responseCode, bool isHashValid)
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
}
