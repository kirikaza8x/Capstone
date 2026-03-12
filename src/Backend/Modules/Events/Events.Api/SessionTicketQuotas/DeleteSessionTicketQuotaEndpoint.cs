using Carter;
using Events.Application.SessionTicketQuotas.Commands.DeleteSessionTicketQuota;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.SessionTicketQuotas;

public class DeleteSessionTicketQuotaEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete(Constants.Routes.QuotaByTicketType, async (
            [FromRoute] Guid eventId,
            [FromRoute] Guid sessionId,
            [FromRoute] Guid ticketTypeId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new DeleteSessionTicketQuotaCommand(eventId, sessionId, ticketTypeId),
                cancellationToken);

            return result.ToOk("Quota deleted successfully.");
        })
        .WithTags(Constants.Tags.Quotas)
        .WithName("DeleteSessionTicketQuota")
        .WithSummary("Delete session ticket quota")
        .WithDescription("Remove quota for a specific zone ticket type in a session.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}