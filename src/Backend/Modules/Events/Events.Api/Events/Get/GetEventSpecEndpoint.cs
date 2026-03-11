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
        app.MapGet(Constants.Routes.EventById + "/spec", async (
            [FromRoute] Guid eventId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetEventSpecQuery(eventId), cancellationToken);
            return result.ToOk();
        })
        .WithTags(Constants.Tags.Events)
        .WithName("GetEventSpec")
        .WithSummary("Get event spec")
        .WithDescription("Retrieve the seatmap spec JSON of an event.")
        .Produces<GetEventSpecResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}