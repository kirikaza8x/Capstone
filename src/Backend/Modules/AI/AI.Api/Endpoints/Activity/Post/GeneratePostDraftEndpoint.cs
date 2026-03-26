using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Application.Abstractions.Authentication;
using Marketing.Application.Posts.Commands;

namespace Marketing.Api.Features.Posts.GeneratePost;

public class GeneratePostDraftEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/posts/generate/{eventId:guid}", async (
            Guid eventId,
            ISender sender,
            ICurrentUserService currentUser,
            CancellationToken cancellationToken) =>
        {
            // Build command
            var command = new GeneratePostDraftCommand(
                EventId: eventId,
                OrganizerId: currentUser.UserId
            );

            // Send through MediatR
            var result = await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Posts")
        .WithName("GeneratePostDraft")
        .WithSummary("Generate a post draft using Gemini based on event details");
    }
}
