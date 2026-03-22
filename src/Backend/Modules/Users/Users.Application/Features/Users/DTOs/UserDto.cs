using System.ComponentModel;
using System.Text.Json.Serialization;
using Shared.Application.DTOs;
using Users.Domain.Enums;

namespace Users.Application.Features.Users.Dtos;

public record BindGoogleRequestDto(string IdToken);
public class UserResponseDto : BaseDto<Guid>
{
    public string? Email { get; set; }
    public string UserName { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime? Birthday { get; set; }
    public Gender? Gender { get; set; }
    public IEnumerable<string>? Roles { get; set; }
}

public class LoginRequestDto
{
    [DefaultValue("admin")]
    public string EmailOrUserName { get; set; } = default!;

    [DefaultValue("123456789")]
    public string Password { get; set; } = default!;

    // Device information metadata
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
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
    public string? FirstName { get; set; } = default!;
    [DefaultValue("User")]
    public string? LastName { get; set; } = default!;

    [DefaultValue("0123456789")]
    public string? PhoneNumber { get; set; } = default!;

    [DefaultValue("some where in vietnam of course lmao")]
    public string? Address { get; set; } = default!;
}

public class GoogleLoginRequestDto
{
    [DefaultValue("server id token")]
    public string IdToken { get; set; } = default!;

    // Optional device information
    public string? DeviceName { get; set; }
}



public record UserProfileDto
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = default!;
    public string? Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public DateTime? Birthday { get; init; }
    public Gender? Gender { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
    public string? Description { get; init; }
    public string? SocialLink { get; init; }
    public string? ProfileImageUrl { get; init; }
    public UserStatus Status { get; init; }
    public IEnumerable<string> Roles { get; init; } = new List<string>();
}





public class UpdateProfileRequestDto
{
    [DefaultValue("d3f8a1b2-1234-5678-9abc-def012345678")]
    public Guid UserId { get; set; }

    [DefaultValue("John")]
    public string? FirstName { get; set; }

    [DefaultValue("Doe")]
    public string? LastName { get; set; }

    [DefaultValue("2000-01-01T00:00:00Z")]
    public DateTime? Birthday { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Gender? Gender { get; set; }

    [DefaultValue("+1234567890")]
    public string? Phone { get; set; }

    [DefaultValue("123 Main St, Springfield")]
    public string? Address { get; set; }

    [DefaultValue("Software engineer passionate about clean code.")]
    public string? Description { get; set; }

    [DefaultValue("https://linkedin.com/in/johndoe")]
    public string? SocialLink { get; set; }

    [DefaultValue("https://cdn.example.com/images/johndoe.png")]
    public string? ProfileImageUrl { get; set; }
}
