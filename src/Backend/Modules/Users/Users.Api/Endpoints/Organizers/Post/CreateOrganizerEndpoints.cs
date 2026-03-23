// using Carter;
// using MediatR;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Routing;
// using Shared.Api.Results;
// using Shared.Domain.Abstractions;
// using Users.Application.Features.Organizers.Commands;
// using Users.Application.Features.Organizers.Dtos;

// namespace Users.Api.Organizers;

// public class CreateOrganizerEndpoints : ICarterModule
// {
//     public void AddRoutes(IEndpointRouteBuilder app)
//     {
//         app.MapPost("api/organizers/profile", async (
//             [FromBody] CreateOrganizerProfileRequestDto requestDto,
//             ISender sender,
//             CancellationToken cancellationToken) =>
//         {
//             var command = new CreateOrganizerProfileCommand(requestDto.Type);
//             Result<Guid> result = await sender.Send(command, cancellationToken);

//             return result.ToOk();
//         })
//         .WithTags("Organizers")
//         .WithName("CreateOrganizerProfile")
//         .WithSummary("Create organizer profile")
//         .WithDescription("Creates a new organizer profile for the current user")
//         .Produces<Guid>(StatusCodes.Status200OK)
//         .ProducesProblem(StatusCodes.Status400BadRequest)
//         .ProducesProblem(StatusCodes.Status404NotFound);
//     }
// }
