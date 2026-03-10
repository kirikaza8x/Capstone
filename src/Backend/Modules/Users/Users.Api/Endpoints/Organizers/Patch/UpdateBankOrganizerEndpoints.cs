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

public class UpdateBankOrganizerEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapPatch("api/organizers/bank", async (
            [FromBody] UpdateOrganizerBankRequestDto requestDto,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateOrganizerBankCommand(
                requestDto.AccountName,
                requestDto.AccountNumber,
                requestDto.BankCode,
                requestDto.Branch
            );

            Result result = await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Organizers")
        .WithName("UpdateOrganizerBank")
        .WithSummary("Update organizer bank info")
        .WithDescription("Updates the bank information of the organizer profile")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
