using Carter;
using Events.Application.Events.Commands.UpdateEventSpec;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using System.Text.Json;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Patch;

public class UpdateEventSpecEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{Constants.Routes.EventById}/spec", async (
            Guid eventId,
            [FromBody] JsonDocument spec,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateEventSpecCommand(eventId, spec),
                cancellationToken);

            return result.ToOk("Add spec successfully!");
        })
        .WithTags(Constants.Tags.Events)
        .WithName("UpdateEventSpec")
        .WithSummary("Update event seatmap spec")
        .WithDescription("Updates the seatmap spec JSON and automatically parses it into areas and seats.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
        //.RequireRoles(Roles.Organizer);
    }
}