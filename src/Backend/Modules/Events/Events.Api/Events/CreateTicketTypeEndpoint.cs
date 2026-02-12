using Carter;
using Events.Application.Events.Commands.CreateTicketType;
using Events.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace Events.Api.Events;

public sealed record CreateTicketTypeRequest(
    string Name,
    decimal Price,
    int Quantity,
    AreaType Type,
    Guid? AreaId);

public class CreateTicketTypeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Constants.Routes.TicketTypes, async (
            [FromRoute] Guid eventId,
            [FromRoute] Guid sessionId,
            [FromBody] CreateTicketTypeRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new CreateTicketTypeCommand(
                    sessionId,
                    request.Name,
                    request.Price,
                    request.Quantity,
                    request.Type,
                    request.AreaId),
                cancellationToken);

            return result.ToCreated(
                $"/api/events/{eventId}/sessions/{sessionId}/ticket-types/{result.Value}",
                "Ticket type created successfully.");
        })
        .WithTags(Constants.TicketTypes)
        .WithName("CreateTicketType")
        .WithSummary("Create a new ticket type")
        .WithDescription("Creates a new ticket type for a session.")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}