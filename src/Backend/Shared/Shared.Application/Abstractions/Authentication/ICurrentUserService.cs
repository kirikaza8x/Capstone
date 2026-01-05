using Shared.Application.DTOs;

namespace Shared.Application.Abstractions.Authentication
{
    public interface ICurrentUserService
{
    Guid UserId { get; }
    string? Email { get; }
    string? Name { get; }
    IEnumerable<string> Roles { get; }
    string? Jti { get; }

    CurrentUserDto? GetCurrentUser();
}

}