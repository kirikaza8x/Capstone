using Carter;
using Events.Application.Events.DTOs;
using Events.Application.Events.Queries.GetTicketTypes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace Events.Api.Events.Get;

public class GetTicketTypesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.TicketTypes, async (
            [FromRoute] Guid eventId,
            [FromQuery] Guid eventSessionId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetTicketTypesQuery(eventId, eventSessionId),
                cancellationToken);

            return result.ToOk();
        })
        .WithTags(Constants.Tags.TicketTypes)
        .WithName("GetTicketTypes")
        .WithSummary("Get ticket types")
        .WithDescription("Retrieve all ticket types of an event with remaining quantity for a specific session.")
        .Produces<ApiResult<IReadOnlyList<TicketTypeDto>>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
