//using Carter;
//using MediatR;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Routing;
//using Products.Application.Products.Query.GetProducts;
//using Shared.Api.Results;
//using Shared.Domain.Abstractions;
//using Shared.Domain.Pagination;

//namespace Products.Api.Products;

//public sealed record GetProductsRequest(
//    int PageNumber = 1,
//    int PageSize = 10,
//    string? SearchTerm = null,
//    string? SortColumn = null,
//    string? SortOrder = null
//);

//public sealed class GetProductsEndpoint : ICarterModule
//{
//    public void AddRoutes(IEndpointRouteBuilder app)
//    {
//        app.MapGet("api/products", async (
//            [AsParameters] GetProductsRequest request,
//            ISender sender,
//            CancellationToken cancellationToken) =>
//        {
//            Result<PagedResult<ProductResponse>> result = await sender.Send(
//                new GetProductsQuery
//                {
//                    PageNumber = request.PageNumber,
//                    PageSize = request.PageSize,
//                    SearchTerm = request.SearchTerm,
//                    SortColumn = request.SortColumn,
//                    SortOrder = request.SortOrder
//                },
//                cancellationToken);

//            return result.ToOk();
//        })
//        .WithTags(Constants.Products)
//        .WithName("GetProducts")
//        .WithSummary("Get paginated list of products")
//        .WithDescription("Retrieves a paginated list of products with optional search and sorting")
//        .Produces<PagedResult<ProductResponse>>(StatusCodes.Status200OK);
//    }
//}