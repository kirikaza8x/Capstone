using Carter;
using Events.Application.Events.Commands.UpdateEventSettings;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace Events.Api.Events;

public sealed record UpdateEventSettingsRequest(
    bool IsEmailReminderEnabled,
    string? UrlPath);

public class UpdateEventSettingsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(Constants.Routes.EventById + "/settings", async (
            [FromRoute] Guid eventId,
            [FromBody] UpdateEventSettingsRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateEventSettingsCommand(
                    eventId,
                    request.IsEmailReminderEnabled,
                    request.UrlPath),
                cancellationToken);

            return result.ToOk("Event settings updated successfully.");
        })
        .WithTags(Constants.Events)
        .WithName("UpdateEventSettings")
        .WithSummary("Update event settings")
        .WithDescription("Update event settings including email reminder and custom URL path.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}