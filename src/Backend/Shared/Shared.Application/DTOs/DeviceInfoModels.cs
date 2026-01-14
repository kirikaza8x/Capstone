namespace Shared.Application.DTOs;

public class DeviceInfo
{
    public string DeviceId { get; set; } = default!;
    public string DeviceName { get; set; } = default!;
    public string Browser { get; set; } = default!;
    public string OperatingSystem { get; set; } = default!;
    public string DeviceType { get; set; } = default!;
    public string? BrowserVersion { get; set; }
    public string? OSVersion { get; set; }
    public string? UserAgent { get; set; }
}