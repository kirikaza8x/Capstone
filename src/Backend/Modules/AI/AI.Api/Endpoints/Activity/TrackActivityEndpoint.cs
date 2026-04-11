using AI.Application.Features.Recommendations.DTOs;
using AI.Application.Features.Tracking.Commands;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;

namespace AI.Api.Features.Activity
{
    public class TrackActivityEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {

            app.MapPost("api/activity/tracking", async (
                [FromBody] TrackActivityRequestDto request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new TrackActivityCommand(
                    request.UserId,
                    request.ActionType,
                    request.TargetId,
                    request.TargetType,
                    request.Metadata
                );

                Result<bool> result = await sender.Send(command, cancellationToken);
                return result.ToOk();
            })
            .WithTags("Activity")
            .WithName("TrackActivity")
            .WithSummary("Track a user interaction")
            .WithDescription("Logs user behavior (clicks, views) to improve recommendations.")
            .Produces<bool>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        }
    }

}
