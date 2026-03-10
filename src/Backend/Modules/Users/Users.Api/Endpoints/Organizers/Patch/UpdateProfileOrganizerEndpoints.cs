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

public class UpdateProfileOrganizerEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        
        // Update Organizer Profile
        app.MapPatch("api/organizers/profile", async (
            [FromBody] UpdateOrganizerProfileRequestDto requestDto,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateOrganizerProfileCommand(
                requestDto.Logo,
                requestDto.DisplayName,
                requestDto.Description,
                requestDto.Address,
                requestDto.SocialLink,
                requestDto.BusinessType,
                requestDto.TaxCode,
                requestDto.IdentityNumber,
                requestDto.CompanyName
            );

            Result result = await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Organizers")
        .WithName("UpdateOrganizerProfile")
        .WithSummary("Update organizer profile")
        .WithDescription("Updates the organizer profile information of the current user")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

    }
}
