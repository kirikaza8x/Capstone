using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Enums;

namespace Users.Api.Organizers;

public class CreateFullOrganizerEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/organizers/profile/full", async (
            [FromBody] CreateFullOrganizerProfileRequestDto requestDto,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateFullOrganizerProfileCommand(
                requestDto.Type,
                requestDto.BusinessInfo,
                requestDto.BankInfo
            );

            Result<Guid> result = await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Organizers")
        .WithName("CreateFullOrganizerProfile")
        .WithSummary("Create full organizer profile")
        .WithDescription("Creates a new fully populated organizer profile for the current user")
        .Produces<Guid>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public record CreateFullOrganizerProfileRequestDto(
    OrganizerType Type,
    OrganizerBusinessInfoDto BusinessInfo,
    OrganizerBankInfoDto BankInfo
);