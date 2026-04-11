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

namespace Users.Api.Endpoints.Users.Post;

public class LoginUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/login", async (
            [FromBody] LoginRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result<LoginResponseDto> result = await sender.Send(
                new LoginUserCommand(
                    request.EmailOrUserName,
                    request.Password,
                    request.DeviceName,
                    request.IpAddress,
                    request.UserAgent),
                cancellationToken);

            return result.ToOk();
        })
        .RequireRateLimiting(RateLimitPolicies.Auth)
        .WithTags("Authentication")
        .WithName("LoginUser")
        .WithSummary("Login a user")
        .WithDescription("Authenticates a user and returns access/refresh tokens along with user info")
        .Produces<LoginResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
