using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Dtos;

namespace Users.Api.Users.Post;

public class RegisterUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/register", async (
            [FromBody] RegisterRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result<UserResponseDto> result = await sender.Send(
                new RegisterUserCommand(
                    request.Email,
                    request.UserName,
                    request.Password,
                    request.FirstName,
                    request.LastName,
                    request.PhoneNumber,
                    request.Address),
                cancellationToken);

            return result.ToOk();
        })
        .WithTags("Authentication")
        .WithName("RegisterUser")
        .WithSummary("Register a new user")
        .WithDescription("Creates a new user account with email, username, password, and profile details")
        .Produces<UserResponseDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
