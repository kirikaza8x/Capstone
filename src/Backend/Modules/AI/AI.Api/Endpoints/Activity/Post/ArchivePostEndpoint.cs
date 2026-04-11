using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Marketing.Application.Posts.Commands;
using Shared.Application.Abstractions.Authentication;
namespace Marketing.Api.Features.Posts.ArchivePost;

public class ArchivePostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/posts/{postId:guid}", async (
            Guid postId,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken) =>
        {
            var command = new ArchivePostCommand(
                postId,
                currentUser.UserId
            );

            var result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .RequireAuthorization()
        .WithTags("Posts")
        .WithName("ArchivePost")
        .WithSummary("Archive a post");
    }
}