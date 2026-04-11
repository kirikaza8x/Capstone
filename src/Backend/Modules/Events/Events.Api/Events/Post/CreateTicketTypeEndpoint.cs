using Carter;
using Events.Application.Events.Commands.CreateTicketType;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Post;

public sealed record CreateTicketTypeRequest(string Name, int Quantity, decimal Price);

public class CreateTicketTypeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Constants.Routes.OrganizerTicketTypes, async (
            [FromRoute] Guid eventId,
            [FromBody] CreateTicketTypeRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new CreateTicketTypeCommand(eventId, request.Name, request.Quantity, request.Price),
                cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToCreated(
                $"/api/events/{eventId}/ticket-types/{result.Value}",
                "Ticket type created successfully.");
        })
        .WithTags(Constants.Tags.TicketTypes)
        .WithName("CreateTicketType")
        .WithSummary("Create a new ticket type")
        .WithDescription("Creates a new ticket type for an event. Assign area separately after seatmap is created.")
        .Produces<ApiResult<Guid>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Organizer);
    }
}
