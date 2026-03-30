using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;
using Events.Application.Events.Queries.GetEventSpecForChecking;

namespace Events.Api.Events.Get;

public class GetEventSpecForCheckingEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.StaffEventSpec, async (
            Guid eventId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetEventSpecForCheckingQuery(eventId);
            var result = await sender.Send(query, cancellationToken);

            return result.IsFailure ? result.ToProblem() : result.ToOk();
        })
        .WithTags(Constants.Tags.EventForStaff)
        .WithName("GetEventSpecForChecking")
        .WithSummary("Get event specification and map image")
        .WithDescription("Retrieves the JSON spec and image URL for the event layout, used by staff for reference.")
        .Produces<ApiResult<GetEventSpecForCheckingResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Staff);
    }
}
