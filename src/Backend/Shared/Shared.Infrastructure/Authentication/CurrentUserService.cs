using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.DTOs;

namespace Shared.Infrastructure.Authentication;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    private HttpContext? Context => _httpContextAccessor.HttpContext;
    private ClaimsPrincipal? User => Context?.User;

    public Guid UserId => GetGuidClaim(ClaimTypes.NameIdentifier);

    public string? Email => GetClaimValue(ClaimTypes.Email);

    public string? Name => GetClaimValue(ClaimTypes.Name);

    public IEnumerable<string> Roles =>
        User?.FindAll(ClaimTypes.Role)
             .Select(c => c.Value)
             .Where(role => !string.IsNullOrWhiteSpace(role))
             .Distinct()
        ?? Enumerable.Empty<string>();

    public string? Jti => GetClaimValue("jti");

    public string? IpAddress => Context?.Connection?.RemoteIpAddress?.ToString();

    public string? UserAgent => Context?.Request?.Headers["User-Agent"].ToString();

    public string? DeviceId => GetClaimValue("DeviceId") ?? GetDeviceIdFromHeader();

    public CurrentUserDto GetCurrentUser() => new()
    {
        UserId = UserId,
        Email = Email,
        Name = Name,
        Roles = Roles.ToList(),
        Jti = Jti,
        IpAddress = IpAddress,
        DeviceId = DeviceId
    };

    private string? GetClaimValue(string claimType)
    {
        return User?.FindFirst(claimType)?.Value;
    }

    private Guid GetGuidClaim(string claimType)
    {
        var value = GetClaimValue(claimType);
        return Guid.TryParse(value, out var guid) ? guid : Guid.Empty;
    }

    private string? GetDeviceIdFromHeader()
    {
        // Allow client to send device ID via custom header
        return Context?.Request?.Headers["X-Device-ID"].ToString();
    }
}