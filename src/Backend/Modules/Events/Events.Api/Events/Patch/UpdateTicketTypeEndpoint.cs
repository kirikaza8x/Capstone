using Carter;
using Events.Application.Events.Commands.UpdateTicketType;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Patch;

public sealed record UpdateTicketTypeRequest(string Name, int Quantity, decimal Price);

public class UpdateTicketTypeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch(Constants.Routes.TicketTypeById, async (
            [FromRoute] Guid eventId,
            [FromRoute] Guid ticketTypeId,
            [FromBody] UpdateTicketTypeRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateTicketTypeCommand(eventId, ticketTypeId, request.Name, request.Quantity, request.Price),
                cancellationToken);

            return result.ToOk("Ticket type updated successfully.");
        })
        .WithTags(Constants.Tags.TicketTypes)
        .WithName("UpdateTicketType")
        .WithSummary("Update ticket type")
        .WithDescription("Update name and price of an existing ticket type.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}
