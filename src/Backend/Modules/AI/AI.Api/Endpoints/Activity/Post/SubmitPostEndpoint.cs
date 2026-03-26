using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Marketing.Application.Posts.Commands;
using Shared.Application.Abstractions.Authentication;

namespace Marketing.Api.Features.Posts.SubmitPost;

public class SubmitPostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/posts/{postId:guid}/submit", async (
            Guid postId,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken) =>
        {
            var command = new SubmitPostCommand(
                postId,
                currentUser.UserId
            );

            var result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .RequireAuthorization()
        .WithTags("Posts")
        .WithName("SubmitPost")
        .WithSummary("Submit post for admin review");
    }
}