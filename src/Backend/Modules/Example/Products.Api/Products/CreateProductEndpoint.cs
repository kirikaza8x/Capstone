using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Products.Application.Products.Commands.CreateProduct;
using Shared.Api.Results;
using Shared.Domain.Abstractions;

namespace Products.Api.Products;

public sealed record CreateProductRequest
(
    string Name,
    string Description,
    decimal Price,
    int Stock
);

public class CreateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/products", async (
            [FromBody] CreateProductRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result<Guid> result = await sender.Send(
                new CreateProductCommand(
                    request.Name,
                    request.Description,
                    request.Price,
                    request.Stock),
                cancellationToken);

            return result.ToCreated(id => $"/api/products/{id}");
        })
        .WithTags(Constants.Products)
        .WithName("CreateProduct")
        .WithSummary("Create a new product")
        .WithDescription("Creates a new product with the specified details")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
