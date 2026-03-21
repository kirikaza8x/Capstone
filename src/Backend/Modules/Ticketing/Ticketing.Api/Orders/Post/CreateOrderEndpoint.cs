using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Ticketing.Application.Orders.Commands.CreateOrder;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Orders.Post;

public sealed record CreateOrderTicketRequest(
    Guid EventSessionId,
    Guid TicketTypeId,
    Guid? SeatId);

public sealed record CreateOrderRequest(
    List<CreateOrderTicketRequest> Tickets);

public class CreateOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Constants.Routes.Orders, async (
            [FromBody] CreateOrderRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var items = request.Tickets
                .Select(x => new CreateOrderTicketItem(
                    x.EventSessionId,
                    x.TicketTypeId,
                    x.SeatId))
                .ToList();

            var command = new CreateOrderCommand(items);

            var result = await sender.Send(command, cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToCreated(
                $"{Constants.Routes.Orders}/{result.Value}",
                "Order created successfully.");
        })
        .WithTags(Constants.Tags.Orders)
        .WithName("CreateOrder")
        .WithSummary("Create a pending order")
        .WithDescription("Creates a pending order with server-side inventory validation and redis lock.")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireRoles(Roles.AttendeeAndOrganizer);
    }
}
