// File: Marketing.Api/Features/Posts/PostPublicEndpoints.cs
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Marketing.Application.Posts.Dtos;

namespace Marketing.Api.Features.Posts;

public class PostPublicEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/public/posts")
            .AllowAnonymous()
            .WithTags("Posts - Public");

        // ─────────────────────────────────────────────────────────────
        // Get Published Post by ID (Public post page)
        // ─────────────────────────────────────────────────────────────
        group.MapGet("/{postId:guid}", async (
            Guid postId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPublicPostByIdQuery(
                PostId: postId
            );

            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithName("GetPublishedPostById")
        .WithSummary("Get a published post by ID")
        .WithDescription("Returns a single published post visible to attendees.")
        .Produces<PostPublicDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}