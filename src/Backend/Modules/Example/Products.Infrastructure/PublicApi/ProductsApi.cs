using Products.Domain.Products;
using Products.PublicApi;
using Products.PublicApi.Dtos;

namespace Products.Infrastructure.PublicApi;

internal sealed class ProductsApi : IProductsApi
{
    private readonly IProductRepository _productRepository;

    public ProductsApi(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);

        return product is null
            ? null
            : new ProductDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Stock,
                product.IsActive);
    }

    public async Task<IReadOnlyList<ProductDto>> GetByIdsAsync(
        IEnumerable<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);

        return products
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Stock,
                p.IsActive))
            .ToList();
    }

    public async Task<bool> ExistsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        return product is not null && product.IsActive;
    }

    public async Task<bool> IsInStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        return product is not null && product.Stock >= quantity;
    }

    public async Task<bool> ReduceStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);

        if (product is null || product.Stock < quantity)
            return false;

        product.UpdateStock(-quantity);
        _productRepository.Update(product);

        return true;
    }
}