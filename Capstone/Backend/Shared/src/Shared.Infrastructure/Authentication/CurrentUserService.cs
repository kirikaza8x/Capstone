using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.DTOs;

namespace Shared.Infrastructure.Authentication
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId =>
            Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)
                ? id
                : Guid.Empty;

        public string? Email =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        public string? Name =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        public IEnumerable<string> Roles =>
            _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(r => r.Value) ?? Enumerable.Empty<string>();

        public string? Jti =>
            _httpContextAccessor.HttpContext?.User?.FindFirst("jti")?.Value;

        public CurrentUserDto GetCurrentUser()
        {
            return new CurrentUserDto
            {
                UserId = UserId,
                Email = Email,
                Name = Name,
                Roles = Roles.ToList(),
                Jti = Jti
            };
        }
    }
}
