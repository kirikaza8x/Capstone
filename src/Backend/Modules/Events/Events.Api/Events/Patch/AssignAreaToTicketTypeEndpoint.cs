using Carter;
using Events.Application.Events.Commands.AssignAreaToTicketType;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Patch;

public sealed record AssignAreaToTicketTypeRequest(Guid AreaId);

public class AssignAreaToTicketTypeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch(Constants.Routes.TicketTypeById + "/assign-area", async (
            [FromRoute] Guid eventId,
            [FromRoute] Guid ticketTypeId,
            [FromBody] AssignAreaToTicketTypeRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new AssignAreaToTicketTypeCommand(eventId, ticketTypeId, request.AreaId),
                cancellationToken);

            return result.ToOk("Area assigned to ticket type successfully.");
        })
        .WithTags(Constants.Tags.TicketTypes)
        .WithName("AssignAreaToTicketType")
        .WithSummary("Assign area to ticket type")
        .WithDescription("Assign a seatmap area to a ticket type. Area must belong to the same event.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}