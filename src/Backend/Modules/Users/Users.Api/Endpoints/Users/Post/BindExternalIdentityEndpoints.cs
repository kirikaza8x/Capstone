using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Dtos;

namespace Users.Api.Users;

public class BindGoogleEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/me/external-identities/google", async (
            [FromBody] BindGoogleRequestDto requestDto,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new BindGoogleCommand(requestDto.IdToken);
            Result result = await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        // .RequireAuthorization()  User must be logged in via password to link
        .WithTags("Users")
        .WithName("BindGoogleIdentity")
        .WithSummary("Link Google account")
        .WithDescription("Binds a Google identity to the currently authenticated user profile")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}