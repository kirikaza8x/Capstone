using Carter;
using Events.Application.SessionTicketQuotas.Queries.GetSessionTicketQuotas;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.SessionTicketQuotas;

public class GetSessionTicketQuotasEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.Quotas, async (
            [FromRoute] Guid eventId,
            [FromRoute] Guid sessionId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetSessionTicketQuotasQuery(eventId, sessionId), cancellationToken);
            return result.ToOk();
        })
        .WithTags(Constants.Tags.Quotas)
        .WithName("GetSessionTicketQuotas")
        .WithSummary("Get session ticket quotas")
        .WithDescription("Retrieve all zone ticket quotas for a session.")
        .Produces<IReadOnlyList<SessionTicketQuotaResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}