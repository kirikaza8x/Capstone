// File: Marketing.Api/Features/Posts/PostPublicEndpoints.cs
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Marketing.Application.Posts.Dtos;
using Marketing.Application.Posts.Queries;

namespace Marketing.Api.Features.Posts;

public class PostPublicEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/public/posts")
            .AllowAnonymous()
            .WithTags("Posts - Public");

        // ─────────────────────────────────────────────────────────────
        // Get Published Posts by Event (Public event page)
        // ─────────────────────────────────────────────────────────────
        group.MapGet("/events/{eventId:guid}", async (
            Guid eventId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPostsByEventQuery(
                EventId: eventId,
                RequesterId: Guid.Empty,
                IsOrganizer: false,
                IncludeDrafts: false
            );

            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithName("GetPublishedPostsByEvent")
        .WithSummary("Get published posts for an event")
        .WithDescription("Returns only Published posts visible to attendees on the event page.")
        .Produces<IReadOnlyList<PostPublicDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}