
namespace Users.Application.Features.Users.Dtos;

public record LoginResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfoDto User,
    string? DeviceId = null,
    string? DeviceName = null
);

public record UserInfoDto(
    Guid UserId,
    string Name,
    string UserName,
    string? Email,
    IEnumerable<string> Roles
);

public record TokenValidationResult(
    bool IsValid,
    Guid? UserId = null,
    string? UserName = null,
    string? Email = null,
    IEnumerable<string>? Roles = null,
    string? DeviceId = null
);

public class RefreshTokenRequestDto
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public string? DeviceId { get; set; }  // Device identifier
    // public string? DeviceName { get; set; }  // Optional friendly name
    // public string? IpAddress { get; set; }  // Optional IP address
    // public string? UserAgent { get; set; }  // Optional User-Agent string
}

public record UserSessionDto(
    string DeviceId,
    string? DeviceName,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsCurrentDevice,
    string? IpAddress = null,
    string? UserAgent = null
);
