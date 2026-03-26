// File: Marketing.Api/Features/Posts/PostManagementEndpoints.cs
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

public class PostManagementEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/posts")
            .RequireAuthorization()
            .WithTags("Posts");

        // ─────────────────────────────────────────────────────────────
        // Create Draft (After AI generates content)
        // ─────────────────────────────────────────────────────────────
        group.MapPost("/", async (
            CreatePostDraftRequestDto request,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken) =>
        {
            var command = new CreatePostDraftCommand(
                EventId: request.EventId,
                OrganizerId: currentUser.UserId,
                Title: request.Title,
                Body: request.Body,
                PromptUsed: request.PromptUsed,
                AiModel: request.AiModel,
                AiTokensUsed: request.AiTokensUsed
            );

            Result<Guid> result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .WithName("CreatePostDraft")
        .WithSummary("Create a new AI-generated post draft")
        .WithDescription("Called after AI service returns generated content. Creates post in Draft status.")
        .Produces<Guid>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        // ─────────────────────────────────────────────────────────────
        // Update Post (Edit draft or rejected post)
        // ─────────────────────────────────────────────────────────────
        group.MapPut("/{postId:guid}", async (
            Guid postId,
            UpdatePostRequestDto request,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdatePostCommand(
                PostId: postId,
                OrganizerId: currentUser.UserId,
                Title: request.Title,
                Body: request.Body
            );

            Result result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .WithName("UpdatePost")
        .WithSummary("Update post content")
        .WithDescription("Only allowed for posts in Draft or Rejected status.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        // ─────────────────────────────────────────────────────────────
        // Submit for Review
        // ─────────────────────────────────────────────────────────────
        group.MapPost("/{postId:guid}/submit", async (
            Guid postId,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken) =>
        {
            var command = new SubmitPostCommand(
                PostId: postId,
                OrganizerId: currentUser.UserId
            );

            Result result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .WithName("SubmitPost")
        .WithSummary("Submit post for admin review")
        .WithDescription("Transitions post from Draft/Rejected to Pending status.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        // ─────────────────────────────────────────────────────────────
        // Publish (After Approval)
        // ─────────────────────────────────────────────────────────────
        group.MapPost("/{postId:guid}/publish", async (
            Guid postId,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken) =>
        {
            var command = new PublishPostCommand(
                PostId: postId,
                OrganizerId: currentUser.UserId
            );

            Result result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .WithName("PublishPost")
        .WithSummary("Publish approved post to platform")
        .WithDescription("Only allowed for posts in Approved status. Makes post visible to attendees.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        // ─────────────────────────────────────────────────────────────
        // Archive (Soft Delete)
        // ─────────────────────────────────────────────────────────────
        group.MapDelete("/{postId:guid}", async (
            Guid postId,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken) =>
        {
            var command = new ArchivePostCommand(
                PostId: postId,
                OrganizerId: currentUser.UserId
            );

            Result result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .WithName("ArchivePost")
        .WithSummary("Archive a post")
        .WithDescription("Soft-delete a post. Not allowed while post is Pending review.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        // ─────────────────────────────────────────────────────────────
        // Get Posts by Event (Organizer view - includes drafts)
        // ─────────────────────────────────────────────────────────────
        group.MapGet("/events/{eventId:guid}", async (
            Guid eventId,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPostsByEventQuery(
                EventId: eventId,
                RequesterId: currentUser.UserId,
                IsOrganizer: true,
                IncludeDrafts: true
            );

            Result<IReadOnlyList<PostDto>> result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithName("GetPostsByEvent_Organizer")
        .WithSummary("Get all posts for an event (organizer view)")
        .WithDescription("Returns all posts created by the organizer for this event, including drafts.")
        .Produces<IReadOnlyList<PostDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        // ─────────────────────────────────────────────────────────────
        // Get Post by ID (Organizer view - full details with permissions)
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
                IsAdmin: false
            );

            Result<PostDetailDto> result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithName("GetPostById_Organizer")
        .WithSummary("Get post details (organizer view)")
        .WithDescription("Returns full post details with UI permission flags (CanEdit, CanSubmit, etc.).")
        .Produces<PostDetailDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

// ─────────────────────────────────────────────────────────────
// DTOs (public sealed class at bottom of file, per your pattern)
// ─────────────────────────────────────────────────────────────

public sealed class CreatePostDraftRequestDto
{
    public Guid EventId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public string? ImageUrl { get; init; }
    public string? PromptUsed { get; init; }
    public string? AiModel { get; init; }
    public int? AiTokensUsed { get; init; }
}

public sealed class UpdatePostRequestDto
{
    public string? Title { get; init; }
    public string? Body { get; init; }
    public string? ImageUrl { get; init; }
}