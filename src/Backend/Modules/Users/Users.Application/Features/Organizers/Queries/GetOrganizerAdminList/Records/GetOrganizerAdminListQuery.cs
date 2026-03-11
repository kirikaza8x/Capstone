using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Enums;

// public sealed record GetOrganizerAdminListQuery(
//     OrganizerStatus? Status,
//     BusinessType? BusinessType,
//     string? Search
// ) : PagedQuery, IQuery<PagedResult<OrganizerAdminListItemDto>>;





public sealed record GetOrganizerAdminListQuery : PagedQuery, IQuery<PagedResult<OrganizerAdminListItemDto>>
{
    public OrganizerStatus? Status {get ; init;}
    public BusinessType? BusinessType {get ; init;}
    public string? Search {get ; init;}
}

