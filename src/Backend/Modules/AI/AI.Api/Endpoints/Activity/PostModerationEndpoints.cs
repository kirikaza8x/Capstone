// File: Marketing.Api/Features/Posts/PostModerationEndpoints.cs
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Marketing.Application.Posts.Commands;
using Marketing.Application.Posts.Dtos;
using Marketing.Application.Posts.Queries;
using Shared.Application.Abstractions.Authentication;

namespace Marketing.Api.Features.Posts;

public class PostModerationEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/admin/posts")
            .RequireAuthorization("Admin")
            .WithTags("Posts - Admin");

        // ─────────────────────────────────────────────────────────────
        // Approve Post
        // ─────────────────────────────────────────────────────────────
        group.MapPost("/{postId:guid}/approve", async (
            Guid postId,
            ApprovePostRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new ApprovePostCommand(
                PostId: postId,
                AdminId: request.AdminId
            );

            Result result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .WithName("ApprovePost")
        .WithSummary("Approve a post for publishing")
        .WithDescription("Transitions post from Pending to Approved. Organizer can then publish.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // ─────────────────────────────────────────────────────────────
        // Reject Post
        // ─────────────────────────────────────────────────────────────
        group.MapPost("/{postId:guid}/reject", async (
            Guid postId,
            RejectPostRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RejectPostCommand(
                PostId: postId,
                AdminId: request.AdminId,
                Reason: request.Reason
            );

            Result result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .WithName("RejectPost")
        .WithSummary("Reject a post with feedback")
        .WithDescription("Transitions post from Pending to Rejected. Organizer can edit and resubmit.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // ─────────────────────────────────────────────────────────────
        // Force Remove Post (Admin emergency action)
        // ─────────────────────────────────────────────────────────────
        group.MapPost("/{postId:guid}/force-remove", async (
            Guid postId,
            ForceRemovePostRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new ForceRemovePostCommand(
                PostId: postId,
                AdminId: request.AdminId,
                Reason: request.Reason
            );

            Result result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .WithName("ForceRemovePost")
        .WithSummary("Force-remove a published post")
        .WithDescription("Emergency action to archive a post that violates platform policy.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // ─────────────────────────────────────────────────────────────
        // Get Pending Queue (Admin moderation dashboard)
        // ─────────────────────────────────────────────────────────────
        group.MapGet("/pending", async (
            int page,
            int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPendingPostsQuery(
                Page: page,
                PageSize: pageSize
            );

            Result<IReadOnlyList<PostPendingItemDto>> result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithName("GetPendingPosts")
        .WithSummary("Get posts awaiting moderation")
        .WithDescription("Returns FIFO queue of posts in Pending status for admin review.")
        .Produces<IReadOnlyList<PostPendingItemDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        // ─────────────────────────────────────────────────────────────
        // Get Post by ID (Admin view)
        // ─────────────────────────────────────────────────────────────
        group.MapGet("/{postId:guid}", async (
            Guid postId,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPostByIdQuery(
                PostId: postId,
                RequesterId: currentUser.UserId,
                IsAdmin: true
            );

            Result<PostDetailDto> result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithName("GetPostById_Admin")
        .WithSummary("Get post details (admin view)")
        .WithDescription("Returns full post details. Admins can view any post regardless of ownership.")
        .Produces<PostDetailDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

// ─────────────────────────────────────────────────────────────
// DTOs (public sealed class at bottom of file)
// ─────────────────────────────────────────────────────────────

public sealed class ApprovePostRequestDto
{
    public Guid AdminId { get; init; }
}

public sealed class RejectPostRequestDto
{
    public Guid AdminId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public sealed class ForceRemovePostRequestDto
{
    public Guid AdminId { get; init; }
    public string Reason { get; init; } = string.Empty;
}