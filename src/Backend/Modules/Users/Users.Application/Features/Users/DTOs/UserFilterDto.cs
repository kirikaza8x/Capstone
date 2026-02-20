using Shared.Domain.Queries;
using Users.Domain.Enums;

namespace Users.Application.Features.Users.Dtos;

public sealed record UserFilterRequestDto : PagedQuery
{
    public string? Email { get; init; }
    public string? UserName { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public DateOnly? BirthdayFrom { get; init; }
    public DateOnly? BirthdayTo { get; init; }
    public Gender? Gender { get; init; }
    public string? PhoneNumber { get; init; }
    public UserStatus? Status { get; init; }
}
