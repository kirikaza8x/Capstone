using Shared.Application.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Enums;

namespace Users.Application.Features.Users.Queries;

public sealed record GetUsersQuery : AdvancedPagedQuery, IQuery<PagedResult<UserResponseDto>>
{
    public string? Email { get; init; }
    public string? UserName { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public DateTime? BirthdayFrom { get; init; }
    public DateTime? BirthdayTo { get; init; }
    public Gender? Gender { get; init; }
    public string? PhoneNumber { get; init; }
    public UserStatus? Status { get; init; }
}
