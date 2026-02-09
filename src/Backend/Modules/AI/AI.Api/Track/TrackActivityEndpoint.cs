using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Domain.Abstractions;
using AI.Application.Features.Tracking.Commands;
using AI.Application.Features.Recommendations.DTOs;

namespace AI.Api.Features.Tracking
{
    public class TrackActivityEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/ai/tracking", async (
                [FromBody] TrackActivityRequestDto request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                // 1. Map DTO to Command
                var command = new TrackActivityCommand(
                    request.UserId,
                    request.ActionType,
                    request.TargetId,
                    request.TargetType,
                    request.Metadata
                );

                Result<bool> result = await sender.Send(command, cancellationToken);

                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            })
            .WithTags("AI Tracking")
            .WithName("TrackUserActivity")
            .WithSummary("Tracks a user interaction")
            .WithDescription("Logs user behavior (clicks, views) to update personal recommendations.")
            .Produces<bool>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        }
    }
}