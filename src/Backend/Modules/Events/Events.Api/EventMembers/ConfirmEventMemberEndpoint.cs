using Carter;
using Events.Application.EventMembers.Commands.ConfirmEventMember;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.EventMembers;

public sealed class ConfirmEventMemberEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(Constants.Routes.ConfirmEventMembers, async (
            Guid eventId,
            Guid memberId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new ConfirmEventMemberCommand(eventId, memberId),
                cancellationToken);

            return result.IsFailure ? result.ToProblem() : Results.NoContent();
        })
        .WithTags(Constants.Tags.Member)
        .WithName("ConfirmEventMemberInvitation")
        .WithSummary("Confirm an invitation to join the event management team")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
