using Microsoft.EntityFrameworkCore;
using Products.Domain.Products;
using Shared.Infrastructure.Repository;

namespace Products.Infrastructure.Data.Repositories;

internal sealed class ProductRepository(ProductsDbContext context) : RepositoryBase<Product, Guid>(context), IProductRepository
{
    public async Task<Product?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await context.Products
            .FirstOrDefaultAsync(
                p => p.Name == name,
                cancellationToken);
    }
}