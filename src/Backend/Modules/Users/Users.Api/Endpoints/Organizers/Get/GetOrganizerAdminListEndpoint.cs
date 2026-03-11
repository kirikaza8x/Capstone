using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Pagination;
using Users.Application.Features.Organizers.Dtos;
using Users.Application.Features.Organizers.Queries;
using Users.Domain.Enums;

namespace Users.Api.Organizers;

public class GetOrganizerAdminListEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/admin/organizers", async (
            [AsParameters] GetOrganizerAdminListRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetOrganizerAdminListQuery
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SortColumn = request.SortColumn,
                Status=request.Status,
                BusinessType=request.BusinessType,
                Search=request.Search
            };

            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithTags("Admin - Organizers")
        .WithName("GetOrganizerAdminList")
        .WithSummary("Get paged list of organizer profiles for admin")
        .WithDescription("Returns a paginated list of organizer profiles with filters")
        .Produces<PagedResult<OrganizerAdminListItemDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}