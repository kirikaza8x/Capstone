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

    [DefaultValue("1990-01-01")]
    public DateOnly? BirthdayFrom { get; init; }

    [DefaultValue("2999-12-31")]
    public DateOnly? BirthdayTo { get; init; }

    [DefaultValue("Male")]
    public Gender? Gender { get; init; }

    [DefaultValue("")]
    public string? PhoneNumber { get; init; }

    [DefaultValue("Active")]
    public UserStatus? Status { get; init; }
}