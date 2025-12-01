using Shared.Application.DTOs;
using System.ComponentModel;

namespace Users.Application.Features.User.Dtos;

public class UserResponseDto : BaseDto<Guid>
{
    public string? Email { get; set; }
    public string UserName { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public IEnumerable<string>? Roles { get; set; } 
}

public class LoginRequestDto
{
    [DefaultValue("admin")]
    public string EmailOrUserName { get; set; } = default!;
    [DefaultValue("123456789")]
    public string Password { get; set; } = default!;
}

public class RegisterRequestDto
{
    [DefaultValue("admin@example.com")]
    public string Email { get; set; } = default!;
    [DefaultValue("Admin")]
    public string UserName { get; set; } = default!;

    [DefaultValue("123456789")]
    public string Password { get; set; } = default!;
    
    [DefaultValue("Admin")]
    public string FirstName { get; set; } = default!;
    [DefaultValue("User")]
    public string LastName { get; set; } = default!;

    [DefaultValue("0123456789")]
    public string PhoneNumber { get; set; } = default!;

    [DefaultValue("some where in vietnam of course lmao")]
    public string Address { get; set; } = default!;
}

public class GoogleLoginRequestDto
{
    [DefaultValue("server id token")]
    public string IdToken { get; set; } = default!;
}