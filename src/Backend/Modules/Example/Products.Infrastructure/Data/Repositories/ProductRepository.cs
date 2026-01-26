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

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(
       IEnumerable<Guid> ids,
       CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();

        return await context.Products
            .Where(p => idList.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }
}