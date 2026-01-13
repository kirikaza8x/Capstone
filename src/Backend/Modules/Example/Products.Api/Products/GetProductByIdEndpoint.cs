using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Application.Products.Queries.GetProductById;
using Shared.Api.Results;
using Shared.Domain.Abstractions;

namespace Products.Api.Products;

public sealed class GetProductByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/products/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result<ProductDetailResponse> result = await sender.Send(
                new GetProductByIdQuery(id),
                cancellationToken);

            return result.ToOk();
        })
        .WithName("GetProductById")
        .WithSummary("Get product by ID")
        .WithDescription("Retrieves detailed information about a specific product")
        .WithTags("Products")
        .Produces<ProductDetailResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}