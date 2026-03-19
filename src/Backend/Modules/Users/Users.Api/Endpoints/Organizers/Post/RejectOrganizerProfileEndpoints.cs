using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Dtos;
public class RejectOrganizerProfileEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/admin/organizers/reject", async (
            [FromBody] RejectOrganizerProfileRequestDto requestDto,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RejectOrganizerProfileCommand(
                requestDto.UserId,
                requestDto.Reason
            );

            Result result = await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Admin - Organizers")
        .WithName("RejectOrganizerProfile")
        .WithSummary("Reject organizer profile")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
