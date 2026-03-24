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
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    // Sorting
    public string SortColumn { get; init; } = "CreatedAt";
    public string SortOrder { get; init; } = "desc";

    // Filters
    public string? Keyword { get; init; }
    public OrganizerStatus? Status { get; init; }
    public BusinessType? BusinessType { get; init; }

    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
}