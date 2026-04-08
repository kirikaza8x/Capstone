using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Shared.Api.RateLimiting;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Dtos;

namespace Users.Api.Users.Post;

public class RefreshTokenEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/refresh-token", async (
            [FromBody] RefreshTokenRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result<LoginResponseDto> result = await sender.Send(
                new RefreshTokenCommand(
                    request.AccessToken,
                    request.RefreshToken,
                    request.DeviceId,
                    null,
                    null,
                    null),
                cancellationToken);

            return result.ToOk();
        })
        .RequireRateLimiting(RateLimitPolicies.Auth)
        .WithTags("Authentication")
        .WithName("RefreshToken")
        .WithSummary("Refresh user tokens")
        .WithDescription("Validates refresh token and issues new access/refresh tokens")
        .Produces<LoginResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
