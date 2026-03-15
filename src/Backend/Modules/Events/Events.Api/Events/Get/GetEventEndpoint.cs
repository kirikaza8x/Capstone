using Carter;
using Events.Application.Events.Queries.GetEventById;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace Events.Api.Events.Get;

public class GetEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.EventById, async (
            [FromRoute] Guid eventId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetEventQuery(eventId),
                cancellationToken);

            return result.ToOk();
        })
        .WithTags(Constants.Tags.Events)
        .WithName("GetEvent")
        .WithSummary("Get event details")
        .WithDescription("Get detailed information about an event.")
        .Produces<ApiResult<GetEventResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}