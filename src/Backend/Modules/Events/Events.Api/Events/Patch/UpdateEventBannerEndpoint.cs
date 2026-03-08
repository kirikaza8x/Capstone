using Carter;
using Events.Application.Events.Commands.UpdateEventBanner;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace Events.Api.Events.Patch;

public sealed record UpdateEventBannerRequest(string BannerUrl);

public class UpdateEventBannerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(Constants.Routes.EventById + "/banner", async (
            [FromRoute] Guid eventId,
            [FromBody] UpdateEventBannerRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateEventBannerCommand(eventId, request.BannerUrl),
                cancellationToken);

            return result.ToOk("Event banner updated successfully.");
        })
        .WithTags(Constants.Tags.Events)
        .WithName("UpdateEventBanner")
        .WithSummary("Update event banner")
        .WithDescription("Update the banner URL for an event.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}