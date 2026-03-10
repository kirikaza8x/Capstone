using Carter;
using Events.Application.Events.Commands.UpdateEventSettings;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace Events.Api.Events.Patch;

public sealed record UpdateEventSettingsRequest(
    bool IsEmailReminderEnabled,
    string? UrlPath,
    DateTime? TicketSaleStartAt,
    DateTime? TicketSaleEndAt,
    DateTime? EventStartAt,
    DateTime? EventEndAt);

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
                    request.UrlPath,
                    request.TicketSaleStartAt,
                    request.TicketSaleEndAt,
                    request.EventStartAt,
                    request.EventEndAt),
                cancellationToken);

            return result.ToOk("Event settings updated successfully.");
        })
        .WithTags(Constants.Tags.Events)
        .WithName("UpdateEventSettings")
        .WithSummary("Update event settings")
        .WithDescription("Update event settings including email reminder, custom URL path, and event schedule.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}