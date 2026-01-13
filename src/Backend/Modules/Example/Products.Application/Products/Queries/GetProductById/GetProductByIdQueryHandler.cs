using Products.Domain.Products;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace Products.Application.Products.Queries.GetProductById;

internal sealed class GetProductByIdQueryHandler
    : IQueryHandler<GetProductByIdQuery, ProductDetailResponse>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDetailResponse>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductDetailResponse>(
                ProductErrors.NotFound(request.Id));
        }

        var response = new ProductDetailResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CreatedBy,
            product.CreatedAt,
            product.ModifiedBy,
            product.ModifiedAt);

        return Result.Success(response);
    }
}