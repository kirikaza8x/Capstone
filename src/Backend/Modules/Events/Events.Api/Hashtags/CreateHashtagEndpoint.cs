using Carter;
using Events.Application.Hashtags.Commands.CreateHashtag;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Hashtags;

public sealed record CreateHashtagRequest(string Name, string Slug);

public class CreateHashtagEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Constants.Routes.Hashtags, async (
            CreateHashtagRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new CreateHashtagCommand(request.Name, request.Slug),
                cancellationToken);

            return result.ToCreated("GetHashtagById", id => new { hashtagId = id });
        })
        .WithTags(Constants.Tags.Hashtags)
        .WithName("CreateHashtag")
        .WithSummary("Create hashtag")
        .WithDescription("Create a new hashtag.")
        .Produces<int>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireRoles(Roles.AllExceptAttendee);
    }
}