// // File: Marketing.Api/Features/Posts/PostTrackingEndpoints.cs
// using Carter;
// using MediatR;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Routing;
// using Shared.Api.Results;
// using Marketing.Application.Posts.Dtos;
// using Marketing.Application.Posts.Queries;

// namespace Marketing.Api.Features.Posts;

// public class PostTrackingEndpoints : ICarterModule
// {
//     public void AddRoutes(IEndpointRouteBuilder app)
//     {
//         var group = app.MapGroup("api/posts/tracking")
//             .AllowAnonymous()
//             .WithTags("Posts - Tracking");

//         // ─────────────────────────────────────────────────────────────
//         // Validate Tracking Token (POST)
//         // ─────────────────────────────────────────────────────────────
//         group.MapPost("/validate", async (
//             ValidateTokenRequestDto request,
//             ISender sender,
//             CancellationToken cancellationToken) =>
//         {
//             var query = new ValidateTrackingTokenQuery(
//                 Token: request.Token
//             );

//             var result = await sender.Send(query, cancellationToken);
//             return result.ToOk();
//         })
//         .WithName("ValidateTrackingToken")
//         .WithSummary("Validate a post tracking token")
//         .WithDescription("Called when a user clicks a post link. Returns redirect URL and attribution data.")
//         .Produces<TrackingValidationDto>(StatusCodes.Status200OK)
//         .ProducesProblem(StatusCodes.Status400BadRequest);

//         // ─────────────────────────────────────────────────────────────
//         // Alternative: GET endpoint for simpler tracking links
//         // Usage: GET /api/posts/tracking/validate?token=xxx
//         // ─────────────────────────────────────────────────────────────
//         group.MapGet("/validate", async (
//             string token,
//             ISender sender,
//             CancellationToken cancellationToken) =>
//         {
//             var query = new ValidateTrackingTokenQuery(Token: token);

//             var result = await sender.Send(query, cancellationToken);
//             return result.ToOk();
//         })
//         .WithName("ValidateTrackingToken_Get")
//         .WithSummary("Validate tracking token (GET)")
//         .WithDescription("Alternative GET endpoint for simpler tracking link integration.")
//         .Produces<TrackingValidationDto>(StatusCodes.Status200OK)
//         .ProducesProblem(StatusCodes.Status400BadRequest);
//     }
// }

// // ─────────────────────────────────────────────────────────────
// // DTOs (public sealed class at bottom of file)
// // ─────────────────────────────────────────────────────────────

// public sealed class ValidateTokenRequestDto
// {
//     public string Token { get; init; } = string.Empty;
// }