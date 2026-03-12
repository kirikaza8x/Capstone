using Carter;
using Events.Application.Events.Commands.DeleteTicketType;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Delete;

public class DeleteTicketTypeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete(Constants.Routes.TicketTypeById, async (
            [FromRoute] Guid eventId,
            [FromRoute] Guid ticketTypeId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new DeleteTicketTypeCommand(eventId, ticketTypeId),
                cancellationToken);

            return result.ToOk("Ticket type deleted successfully.");
        })
        .WithTags(Constants.Tags.TicketTypes)
        .WithName("DeleteTicketType")
        .WithSummary("Delete ticket type")
        .WithDescription("Delete a ticket type from an event.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}