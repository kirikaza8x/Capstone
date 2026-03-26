using Shared.Application.Abstractions.Authentication;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Marketing.Application.Posts.Commands;
using MediatR;

namespace Marketing.Api.Features.Posts.UpdatePost;

public class UpdatePostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/posts/{postId:guid}", async (
            Guid postId,
            UpdatePostRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdatePostCommand(
                PostId: postId,
                Title: request.Title,
                Body: request.Body,
                Summary: request.Summary,
                ImageUrl: request.ImageUrl,
                Slug: request.Slug,
                PromptUsed: request.PromptUsed,
                AiModel: request.AiModel,
                AiTokensUsed: request.AiTokensUsed,
                AiCost: request.AiCost,
                TrackingToken: request.TrackingToken
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
    public string? Summary { get; init; }
    public string? ImageUrl { get; init; }
    public string? Slug { get; init; }
    public string? PromptUsed { get; init; }
    public string? AiModel { get; init; }
    public int? AiTokensUsed { get; init; }
    public decimal? AiCost { get; init; }
    public string? TrackingToken { get; init; }
}
