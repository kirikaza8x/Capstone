using Carter;
using Events.Application.SessionTicketQuotas.Commands.SetSessionTicketQuota;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.SessionTicketQuotas;

public sealed record SetSessionTicketQuotaRequest(Guid TicketTypeId, int Quantity);

public class SetSessionTicketQuotaEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(Constants.Routes.Quotas, async (
            [FromRoute] Guid eventId,
            [FromRoute] Guid sessionId,
            [FromBody] SetSessionTicketQuotaRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new SetSessionTicketQuotaCommand(eventId, sessionId, request.TicketTypeId, request.Quantity),
                cancellationToken);

            return result.ToOk("Quota set successfully.");
        })
        .WithTags(Constants.Tags.Quotas)
        .WithName("SetSessionTicketQuota")
        .WithSummary("Set session ticket quota")
        .WithDescription("Create or update quota for a zone-type ticket in a session. Only applies to zone areas.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}