namespace Shared.Application.DTOs;

public class CurrentUserDto
{
    public Guid UserId { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public List<string> Roles { get; set; } = new();
    public string? Jti { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; } 
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? DeviceType { get; set; }
    public string? BrowserVersion { get; set; }
    public string? OSVersion { get; set; }
    public string? UserAgent { get; set; }
}

