using Carter;
using Events.Application.Hashtags.Commands.UpdateHashtag;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Hashtags;

public sealed record UpdateHashtagRequest(string Name);

public class UpdateHashtagEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(Constants.Routes.HashtagById, async (
            [FromRoute] int hashtagId,
            UpdateHashtagRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateHashtagCommand(hashtagId, request.Name),
                cancellationToken);

            return result.ToOk("Hashtag updated successfully.");
        })
        .WithTags(Constants.Tags.Hashtags)
        .WithName("UpdateHashtag")
        .WithSummary("Update hashtag")
        .WithDescription("Update an existing hashtag.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireRoles(Roles.AllExceptAttendee);
    }
}