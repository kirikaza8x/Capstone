using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Enums;

public sealed record GetAllOrganizersQuery : PagedQuery,
    IQuery<PagedResult<OrganizerAdminListItemDto>>
{
    public string? Keyword { get; init; }

    public OrganizerStatus? Status { get; init; } 

    public BusinessType? BusinessType { get; init; }

    public DateTime? CreatedFrom { get; init; }

    public DateTime? CreatedTo { get; init; }
}