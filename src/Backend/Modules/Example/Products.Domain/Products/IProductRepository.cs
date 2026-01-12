using Shared.Domain.Data;

namespace Products.Domain.Products;

public interface IProductRepository : IRepository<Product, Guid>
{
    Task<Product?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}