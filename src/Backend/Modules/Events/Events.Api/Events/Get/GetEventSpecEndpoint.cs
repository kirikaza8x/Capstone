using Carter;
using Events.Application.Events.Queries.GetEventSpec;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace Events.Api.Events.Get;

public class GetEventSpecEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet($"{Constants.Routes.EventById}/sessions/{{sessionId:guid}}/spec", async (
            [FromRoute] Guid eventId,
            [FromRoute] Guid sessionId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetEventSpecQuery(eventId, sessionId),
                cancellationToken);

            return result.ToOk();
        })
        .WithTags(Constants.Tags.Events)
        .WithName("GetEventSpec")
        .WithSummary("Get event spec with realtime seat availability")
        .WithDescription("Retrieve seatmap spec and enrich seat status as available/blocked for the target session.")
        .Produces<ApiResult<GetEventSpecResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
