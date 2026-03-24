using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Users.Application.Features.Organizers.Dtos;
using Users.Application.Features.Organizers.Commands;
using Users.Domain.Enums;

namespace Users.Api.Organizers;

public class StartOrUpdateOrganizerProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/organizer/profile", async (
            StartOrUpdateOrganizerProfileReqestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new StartOrUpdateOrganizerProfileCommand(
                request.Type,
                request.BusinessInfo,
                request.BankInfo
            );

            var result = await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        .WithName("CreateFullOrganizerProfile")
        .WithTags("Organizer")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}

public sealed class StartOrUpdateOrganizerProfileReqestDto
{
    public OrganizerType Type { get; init; }

    public OrganizerBusinessInfoDto BusinessInfo { get; init; } = default!;

    public OrganizerBankInfoDto BankInfo { get; init; } = default!;
}
