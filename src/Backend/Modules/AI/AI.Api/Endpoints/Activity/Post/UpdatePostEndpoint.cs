using MediatR;
using Shared.Application.Abstractions.Authentication;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Marketing.Application.Posts.Commands;
namespace Marketing.Api.Features.Posts.UpdatePost;

public class UpdatePostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/posts/{postId:guid}", async (
            Guid postId,
            UpdatePostRequestDto request,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdatePostCommand(
                postId,
                currentUser.UserId,
                request.Title,
                request.Body
            );

            var result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .RequireAuthorization()
        .WithTags("Posts")
        .WithName("UpdatePost")
        .WithSummary("Update post content");
    }
}

public sealed class UpdatePostRequestDto
{
    public string? Title { get; init; }
    public string? Body { get; init; }
    public string? ImageUrl { get; init; }
}