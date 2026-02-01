using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Products.Application.Products.Commands.UpdateProduct;
using Shared.Api.Results;

namespace Products.Api.Products;

public sealed record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int Stock
);

public class UpdateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/products/{id:guid}", async (
            Guid id,
            [FromBody] UpdateProductRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateProductCommand(
                    id,
                    request.Name,
                    request.Description,
                    request.Price,
                    request.Stock),
                cancellationToken);

            return result.ToOk();
        })
        .WithTags(Constants.Products)
        .WithName("UpdateProduct")
        .WithSummary("Update a product")
        .WithDescription("Updates a product with the specified details")
        .Produces<Guid>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}