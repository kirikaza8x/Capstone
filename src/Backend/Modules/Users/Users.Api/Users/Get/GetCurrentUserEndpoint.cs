using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Shared.Application.DTOs;
using Users.Application.Features.Users.Queries;
using Carter;

namespace Users.Api.Users.Get;

public class GetCurrentUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/current", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result<CurrentUserDto> result = await sender.Send(
                new GetCurrentUserQuery(),
                cancellationToken);

            return result.ToOk();
        })
        .WithTags("Users")
        .WithName("GetCurrentUser")
        .WithSummary("Get current user info")
        .WithDescription("Returns information about the currently authenticated user")
        .Produces<CurrentUserDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
