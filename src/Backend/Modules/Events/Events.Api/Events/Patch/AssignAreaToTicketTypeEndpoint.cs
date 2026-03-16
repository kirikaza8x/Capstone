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

public sealed record AssignTicketTypeAreaRequest(
    Guid TicketTypeId,
    Guid AreaId);

public sealed record AssignAreaToTicketTypeRequest(
    IReadOnlyCollection<AssignTicketTypeAreaRequest> Mappings);

public class AssignAreaToTicketTypeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch(Constants.Routes.TicketTypes + "/assign-area", async (
            [FromRoute] Guid eventId,
            [FromBody] AssignAreaToTicketTypeRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var mappings = request.Mappings
                .Select(x => new AssignTicketTypeAreaItem(x.TicketTypeId, x.AreaId))
                .ToList();

            var result = await sender.Send(
                new AssignAreaToTicketTypeCommand(eventId, mappings),
                cancellationToken);

            return result.ToOk("Areas assigned to ticket types successfully.");
        })
        .WithTags(Constants.Tags.TicketTypes)
        .WithName("AssignAreasToTicketTypes")
        .WithSummary("Assign areas to ticket types")
        .WithDescription("Assign area for each ticket type using ticketTypeId-areaId mappings.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}