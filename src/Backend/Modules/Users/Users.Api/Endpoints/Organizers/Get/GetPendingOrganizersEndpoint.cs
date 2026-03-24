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

public class GetPendingOrganizersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/organizers/pending", async (
            [AsParameters] GetPendingOrganizersRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPendingOrganizersQuery
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SortColumn = request.SortColumn,
                SortOrder = request.SortOrder?.ToLower() == "asc"
                    ? SortOrder.Ascending
                    : SortOrder.Descending,
                Keyword = request.Keyword,
                BusinessType = request.BusinessType
            };

            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithName("GetPendingOrganizers")
        .WithTags("Admin - Organizers")
        .Produces<PagedResult<OrganizerAdminListItemDto>>(StatusCodes.Status200OK);
    }
}

public sealed record GetPendingOrganizersRequestDto
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

    public BusinessType? BusinessType { get; init; }
}