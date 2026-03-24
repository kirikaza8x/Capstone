using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Commands.UpdateLogo;

namespace Users.Api.Organizers;

public class UpdateOrganizerLogoEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/organizers/{userId}/logo", async (
            Guid userId,
            IFormFile file,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateLogoImageCommand(userId, new FormFileUpload(file));

            Result<Guid> result = await sender.Send(command, cancellationToken);

            return result.ToOk("Organizer logo updated successfully.");
        })
        .WithTags("Organizers")
        .WithName("UpdateOrganizerLogo")
        .WithSummary("Update organizer draft logo")
        .WithDescription("Uploads and updates the logo for the organizer draft profile (strict aggregate)")
        .Accepts<IFormFile>("multipart/form-data")
        .Produces<Guid>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .DisableAntiforgery();
    }
}