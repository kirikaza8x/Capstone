using System.ComponentModel;
using Shared.Application.Dtos.Queries;
using Users.Domain.Enums;

namespace Users.Application.Features.Users.Dtos;

public sealed record UserFilterRequestDto : PagedRequestDto
{
    [DefaultValue("")]
    public string? Email { get; init; }

    [DefaultValue("")]
    public string? UserName { get; init; }

    [DefaultValue("")]
    public string? FirstName { get; init; }

    [DefaultValue("")]
    public string? LastName { get; init; }

    [DefaultValue("1900-01-01T00:00:00Z")]
    public DateTime? BirthdayFrom { get; init; } = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [DefaultValue("2100-12-31T23:59:59Z")]
    public DateTime? BirthdayTo { get; init; } = new DateTime(2100, 12, 31, 23, 59, 59, DateTimeKind.Utc);

    [DefaultValue("Male")]
    public Gender? Gender { get; init; }

    [DefaultValue("")]
    public string? PhoneNumber { get; init; }

    [DefaultValue("Active")]
    public UserStatus? Status { get; init; }
}

public sealed record UserBaseRequestDto : PagedBaseRequestDto
{
    [DefaultValue("")]
    public string? Email { get; init; }

    [DefaultValue("")]
    public string? UserName { get; init; }

    [DefaultValue("")]
    public string? FirstName { get; init; }

    [DefaultValue("")]
    public string? LastName { get; init; }

    [DefaultValue("1900-01-01T00:00:00Z")]
    public DateTime? BirthdayFrom { get; init; } = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [DefaultValue("2100-12-31T23:59:59Z")]
    public DateTime? BirthdayTo { get; init; } = new DateTime(2100, 12, 31, 23, 59, 59, DateTimeKind.Utc);

    [DefaultValue("Male")]
    public Gender? Gender { get; init; }

    [DefaultValue("")]
    public string? PhoneNumber { get; init; }

    [DefaultValue("Active")]
    public UserStatus? Status { get; init; }
}