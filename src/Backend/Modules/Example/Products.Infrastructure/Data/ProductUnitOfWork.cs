using Products.Domain.Products;
using Shared.Infrastructure.Data;

namespace Products.Infrastructure.Data;

internal class ProductUnitOfWork : UnitOfWorkBase<ProductsDbContext>, IProductUnitOfWork
{
    public ProductUnitOfWork(ProductsDbContext dbContext) : base(dbContext)
    {
    }
}
