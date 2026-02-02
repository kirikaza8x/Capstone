using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Order.Application.Orders.Commands.AddOrderItem;
using Shared.Api.Results;

namespace Order.Api.Orders;

public sealed record AddOrderItemRequest(
    Guid ProductId,
    int Quantity
);

public class AddOrderItemEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/orders/{orderId:guid}/items", async (
            Guid orderId,
            [FromBody] AddOrderItemRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new AddOrderItemCommand(
                    orderId,
                    request.ProductId,
                    request.Quantity),
                cancellationToken);

            return result.ToNoContent();
        })
        .WithTags(Constants.Orders)
        .WithName("AddOrderItem")
        .WithSummary("Add an item to an existing order")
        .WithDescription("Adds a product to a pending order. Only pending orders can be modified.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
