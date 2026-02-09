using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Order.Application.Orders.Commands.CreateOrder;
using Shared.Api.Results;

namespace Order.Api.Orders;

public sealed record CreateOrderRequest(
    Guid CustomerId,
    string CustomerName,
    string ShippingAddress,
    List<OrderItemRequest> Items);

public class CreateOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/orders", async (
            [FromBody] CreateOrderRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new CreateOrderCommand(
                    request.CustomerId,
                    request.CustomerName,
                    request.ShippingAddress,
                    request.Items),
                cancellationToken);

            return result.ToCreated("GetOrderById", id => new { id });
        })
        .WithTags(Constants.Orders)
        .WithName("CreateOrder")
        .WithSummary("Create a new order")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}