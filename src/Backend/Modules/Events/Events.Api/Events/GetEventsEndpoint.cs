using Carter;
using Events.Application.Events.Queries.GetEvents;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Shared.Domain.Pagination;
using Users.PublicApi.Constants;

namespace Events.Api.Events;

public class GetEventsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.Events, async (
            [AsParameters] GetEventsQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                query,
                cancellationToken);

            return result.ToOk();
        })
        .WithTags(Constants.Tags.Events)
        .WithName("GetEvents")
        .WithSummary("Get all events")
        .WithDescription("Get all events with pagination.")
        .Produces<PagedResult<EventResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireRoles(Roles.Admin);
    }
}