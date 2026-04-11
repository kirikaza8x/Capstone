using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Enums;

public sealed record GetPendingOrganizersQuery : PagedQuery,
    IQuery<PagedResult<OrganizerAdminListItemDto>>
{
    public string? Keyword { get; init; }
    public BusinessType? BusinessType { get; init; }
}