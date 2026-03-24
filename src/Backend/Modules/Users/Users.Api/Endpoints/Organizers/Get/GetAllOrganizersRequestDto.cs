using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Pagination;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Enums;
using Shared.Domain.Queries;
using System.ComponentModel;
namespace Users.Api.Organizers;

public class GetAllOrganizersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/organizers", async (
            [AsParameters] GetAllOrganizersRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllOrganizersQuery
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SortColumn = request.SortColumn,
                SortOrder = request.SortOrder?.ToLower() == "asc"
        ? SortOrder.Ascending
        : SortOrder.Descending,
                Keyword = request.Keyword,
                Status = request.Status,
                BusinessType = request.BusinessType,
                CreatedFrom = request.CreatedFrom,
                CreatedTo = request.CreatedTo
            };

            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithName("GetAllOrganizers")
        .WithTags("Admin - Organizers")
        .Produces<PagedResult<OrganizerAdminListItemDto>>(StatusCodes.Status200OK);
    }
}

public sealed record GetAllOrganizersRequestDto
{
    // Paging
    [DefaultValue(1)]
    public int PageNumber { get; init; } = 1;

    [DefaultValue(10)]
    public int PageSize { get; init; } = 10;

    // Sorting
    [DefaultValue("CreatedAt")]
    public string SortColumn { get; init; } = "CreatedAt";

    [DefaultValue("desc")]
    public string SortOrder { get; init; } = "desc";

    // Filters
    public string? Keyword { get; init; }

    public OrganizerStatus? Status { get; init; }

    public BusinessType? BusinessType { get; init; }

    public DateTime? CreatedFrom { get; init; }

    public DateTime? CreatedTo { get; init; }
}