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
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    // Sorting
    public string SortColumn { get; init; } = "CreatedAt";
    public string SortOrder { get; init; } = "desc";

    // Filters
    public string? Keyword { get; init; }
    public BusinessType? BusinessType { get; init; }
}
