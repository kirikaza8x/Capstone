using Microsoft.EntityFrameworkCore;
using Products.Domain.Products;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.Outbox;

namespace Products.Infrastructure.Data;

public sealed class ProductsDbContext(DbContextOptions<ProductsDbContext> options) : DbContextBase(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("products");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductsDbContext).Assembly);
        // apply outbox configuration
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
