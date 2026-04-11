using Carter;
using Events.Application.Hashtags.Commands.DeleteHashtag;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Hashtags.Delete;

public class DeleteHashtagEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete(Constants.Routes.HashtagById, async (
            [FromRoute] int hashtagId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeleteHashtagCommand(hashtagId), cancellationToken);
            return result.ToOk("Hashtag deleted successfully.");
        })
        .WithTags(Constants.Tags.Hashtags)
        .WithName("DeleteHashtag")
        .WithSummary("Delete hashtag")
        .WithDescription("Permanently delete a hashtag.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AdminAndStaff);
    }
}
