using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.DTOs;

namespace Shared.Infrastructure.Authentication;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid UserId => Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
    public string? Email => User?.FindFirstValue(ClaimTypes.Email);
    public string? Name => User?.FindFirstValue(ClaimTypes.Name);
    public IEnumerable<string> Roles => User?.FindAll(ClaimTypes.Role).Select(r => r.Value) ?? [];
    public string? Jti => User?.FindFirstValue("jti");

    public CurrentUserDto GetCurrentUser() => new()
    {
        UserId = UserId,
        Email = Email,
        Name = Name,
        Roles = Roles.ToList(),
        Jti = Jti
    };
}
