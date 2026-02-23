// using Carter;
// using MediatR;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Routing;
// using Shared.Api.Results;
// using Shared.Domain.Pagination;
// using Users.Application.Features.Users.Dtos;
// using Users.Application.Features.Users.Queries;

// namespace Users.Api.Users;

// public class GetUsersSimpleEndpoint : ICarterModule
// {
//     public void AddRoutes(IEndpointRouteBuilder app)
//     {
//         app.MapGet("api/users", async (
//             [AsParameters] UserFilterRequestDto request, 
//             ISender sender,
//             CancellationToken cancellationToken) =>
//         {
//             var query = new GetUsersQuery
//             {
//                 PageNumber = request.PageNumber,
//                 PageSize = request.PageSize,
//                 SearchTerm = request.SearchTerm,
//                 Email = request.Email,
//                 UserName = request.UserName,
//                 Status = request.Status
//             };

//             var result = await sender.Send(query, cancellationToken);
//             return result.ToOk();
//         })
//         .WithTags("Users")
//         .WithName("GetUsersSimple")
//         .WithSummary("Get a simple paged list of users")
//         .WithDescription("Returns a paged list of users with basic filters via query string")
//         .Produces<PagedResult<UserResponseDto>>(StatusCodes.Status200OK)
//         .ProducesProblem(StatusCodes.Status400BadRequest)
//         .ProducesProblem(StatusCodes.Status404NotFound);
//     }
// }
