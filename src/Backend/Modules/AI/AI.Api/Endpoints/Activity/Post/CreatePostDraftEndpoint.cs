using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Marketing.Application.Posts.Commands;
using Shared.Application.Abstractions.Authentication;
namespace Marketing.Api.Features.Posts.CreatePostDraft;

public class CreatePostDraftEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/posts", async (
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

            var result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .RequireAuthorization()
        .WithTags("Posts")
        .WithName("CreatePostDraft")
        .WithSummary("Create a new AI-generated post draft");
    }
}

public sealed class CreatePostDraftRequestDto
{
    public Guid EventId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public string? PromptUsed { get; init; }
    public string? AiModel { get; init; }
    public int? AiTokensUsed { get; init; }
}