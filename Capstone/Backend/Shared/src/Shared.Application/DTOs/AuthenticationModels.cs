// using Shared.Presentation.Common.Attributes;

namespace Shared.Application.DTOs;

// Deprecated: use Application.Users.Commands.LoginUserCommand directly
// public record LoginRequest(string Email, string Password);

public record LoginResponseDto(
    string AccessToken, 
    string RefreshToken, 
    DateTime ExpiresAt, 
    UserInfoDto User
);

public record UserInfoDto   (
    Guid UserId,
    string Name,
    string UserName,
    string? Email,
    IEnumerable<string> Roles
);

// Deprecated: use Application.Users.Commands.RefreshTokenCommand directly
// public record RefreshTokenRequest(string RefreshToken);

public record TokenValidationResult(
    bool IsValid,
    Guid? UserId = null,
    string? UserName = null,
    string? Email = null,
    IEnumerable<string>? Roles = null
);

public class RefreshTokenRequestDto
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}


// [SwaggerIgnoreModel]
public class CurrentUserDto
{
    public Guid UserId { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public List<string> Roles { get; set; } = new();
    public string? Jti { get; set; }
}
