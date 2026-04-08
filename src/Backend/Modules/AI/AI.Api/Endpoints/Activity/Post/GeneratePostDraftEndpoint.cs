using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Shared.Api.RateLimiting;
using Shared.Api.Results;
using Shared.Application.Abstractions.Authentication;

namespace Marketing.Api.Features.Posts.GeneratePost;

public class GeneratePostDraftRequest
{
    public string? UserPromptRequirement { get; set; }
}

public class GeneratePostDraftEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/posts/generate/{eventId:guid}", async (
            Guid eventId,
            ISender sender,
            GeneratePostDraftRequest request,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken) =>
        {
            // Build command
            var command = new GeneratePostDraftCommand(
                EventId: eventId,
                OrganizerId: currentUser.UserId,
                UserPromptRequirement: request.UserPromptRequirement
            );

            var result = await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        .RequireRateLimiting(RateLimitPolicies.AiGenerate)
        .WithTags("Posts")
        .WithName("GeneratePostDraft")
        .WithSummary("Generate a post draft using Gemini based on event details");
    }
}
