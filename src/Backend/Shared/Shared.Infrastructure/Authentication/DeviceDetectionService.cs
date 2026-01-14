using Shared.Application.Abstractions.Authentication;
using Shared.Application.DTOs;
using UAParser;

public class DeviceDetectionService : IDeviceDetectionService
{
    private readonly Parser _uaParser;

    public DeviceDetectionService()
    {
        _uaParser = Parser.GetDefault();
    }

    public DeviceInfo GetDeviceInfo(
        string? userAgent,
        string? ipAddress = null,
        string? existingDeviceId = null)
    {
        var deviceId = existingDeviceId ?? GenerateDeviceId();

        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return new DeviceInfo
            {
                DeviceId = deviceId,
                DeviceName = "Unknown Device",
                Browser = "Unknown",
                OperatingSystem = "Unknown",
                DeviceType = "Unknown",
                UserAgent = userAgent,
                //IpAddress = ipAddress
            };
        }

        var clientInfo = _uaParser.Parse(userAgent);

        var browser = GetBrowserName(clientInfo);
        var os = GetOperatingSystemName(clientInfo);
        var deviceType = GetDeviceType(clientInfo);
        var deviceName = ResolveDeviceName(userAgent);

        return new DeviceInfo
        {
            DeviceId = deviceId,
            DeviceName = deviceName ?? "Unknown Device",
            Browser = browser,
            OperatingSystem = os,
            DeviceType = deviceType,
            BrowserVersion = clientInfo.UA.Major,
            OSVersion = clientInfo.OS.Major,
            UserAgent = userAgent,
            //IpAddress = ipAddress
        };
    }

    public string GenerateDeviceId()
    {
        return Guid.NewGuid().ToString();
    }

    public string? ResolveDeviceName(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return "Unknown Device";

        var clientInfo = _uaParser.Parse(userAgent);
        var browser = GetBrowserName(clientInfo);
        var os = GetOperatingSystemName(clientInfo);
        var deviceFamily = clientInfo.Device.Family;

        if (!string.IsNullOrWhiteSpace(deviceFamily) && deviceFamily != "Other")
        {
            return $"{deviceFamily} ({browser} on {os})";
        }

        return $"{browser} on {os}";
    }

    private string GetBrowserName(ClientInfo clientInfo)
    {
        var browser = clientInfo.UA.Family;
        return browser switch
        {
            "Chrome" => "Chrome",
            "Firefox" => "Firefox",
            "Safari" => "Safari",
            "Edge" => "Edge",
            "Opera" => "Opera",
            "IE" => "Internet Explorer",
            _ => browser ?? "Unknown Browser"
        };
    }

    private string GetOperatingSystemName(ClientInfo clientInfo)
    {
        var os = clientInfo.OS.Family;
        return os switch
        {
            "Windows" => "Windows",
            "Mac OS X" => "macOS",
            "iOS" => "iOS",
            "Android" => "Android",
            "Linux" => "Linux",
            "Ubuntu" => "Ubuntu",
            _ => os ?? "Unknown OS"
        };
    }

    private string GetDeviceType(ClientInfo clientInfo)
    {
        var device = clientInfo.Device.Family;

        if (clientInfo.Device.IsSpider)
            return "Bot";

        if (device.Contains("iPhone") || device.Contains("iPad"))
            return "Mobile";

        if (device.Contains("Android"))
            return "Mobile";

        if (clientInfo.OS.Family == "iOS" || clientInfo.OS.Family == "Android")
            return "Mobile";

        return "Desktop";
    }
}
