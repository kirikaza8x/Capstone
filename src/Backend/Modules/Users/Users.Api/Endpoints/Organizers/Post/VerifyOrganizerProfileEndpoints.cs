using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Commands;
using Users.Application.Features.Organizers.Dtos;

namespace Users.Api.Organizers;
public class VerifyOrganizerProfileEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/admin/organizers/verify", async (
            [FromBody] VerifyOrganizerProfileRequestDto requestDto,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new VerifyOrganizerProfileCommand(requestDto.UserId);

            Result result = await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Admin - Organizers")
        .WithName("VerifyOrganizerProfile")
        .WithSummary("Verify organizer profile")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
