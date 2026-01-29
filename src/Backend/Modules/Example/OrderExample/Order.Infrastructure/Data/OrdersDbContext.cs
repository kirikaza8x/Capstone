using Order.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.Outbox;


namespace Order.Infrastructure.Data;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Constants.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Order.Domain.Orders.Order> Orders => Set<Order.Domain.Orders.Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
}
