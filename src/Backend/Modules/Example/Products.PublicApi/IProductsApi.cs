using Products.PublicApi.Dtos;

namespace Products.PublicApi;

public interface IProductsApi
{
    Task<ProductDto?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductDto>> GetByIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<bool> IsInStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task<bool> ReduceStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
}