using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Dtos;
using Users.Application.Features.Users.Queries;

namespace Users.Api.Users.Get;

public class GetUserByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/user/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result<UserResponseDto> result = await sender.Send(
                new GetUserByIdQuery(id),
                cancellationToken);

            return result.ToOk();
        })
        .WithTags("Users")
        .WithName("GetUserById")
        .WithSummary("Get user by ID")
        .WithDescription("Returns information about a user by their unique identifier")
        .Produces<UserResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
