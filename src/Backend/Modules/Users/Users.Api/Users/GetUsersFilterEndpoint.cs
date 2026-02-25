using Carter;
using MediatR;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Pagination;
using Users.Application.Features.Users.Dtos;
using Users.Application.Features.Users.Queries;

namespace Users.Api.Users;

public class GetUsersFilterEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/search", async (
            [FromBody] UserFilterRequestDto request,
            ISender sender,
            IMapper mapper,
            CancellationToken cancellationToken) =>
        {
            var query = mapper.Map<GetUsersQuery>(request);

            var result = await sender.Send(query, cancellationToken);

            // 3. Return the PagedResult using your Result extension
            return result.ToOk();
        })
        .WithTags("Users")
        .WithName("GetUsers")
        .WithSummary("Get paged list of users")
        .WithDescription("Returns a paged list of users with dynamic filtering, sorting, and search options.")
        .Produces<PagedResult<UserResponseDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
        // .WithOpenApi(); // Ensures Swagger metadata is correctly generated
    }
}