using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Users.Application.Features.Users.Commands.LoginGoogle;
using Users.Application.Features.Users.Dtos;
namespace Users.Api.Endpoints.Users.Post;

public class GoogleLoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/google-login", async (
            [FromBody] GoogleLoginRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new GoogleLoginCommand(request.IdToken, request.DeviceName);

            var result = await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Authentication")
        .WithName("GoogleLogin")
        .Produces<LoginResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
