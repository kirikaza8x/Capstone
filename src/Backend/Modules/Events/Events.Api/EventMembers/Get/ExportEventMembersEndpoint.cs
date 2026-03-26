using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Events.Application.EventMembers.Queries.ExportEventMembers;
using Users.PublicApi.Constants;
using Shared.Api.Extensions;

namespace Events.Api.EventMembers.Get;

public class ExportEventMembersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.ExportEventMembers, async (
            [FromRoute] Guid eventId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new ExportEventMembersQuery(eventId);
            var result = await sender.Send(query, cancellationToken);
            return Results.File(result.Value, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "event-members.xlsx");
        })
        .WithTags(Constants.Tags.EventForOrganizer)
        .WithName("ExportEventMembers")
        .WithSummary("Export event members to Excel")
        .WithDescription("Exports all event members into an Excel file")
        .RequireRoles("Organizer");
    }
}
