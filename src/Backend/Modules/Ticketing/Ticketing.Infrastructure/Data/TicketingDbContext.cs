using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Outbox;
using Ticketing.Domain.Entities;

namespace Ticketing.Infrastructure.Data;

public sealed class TicketingDbContext(DbContextOptions<TicketingDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderTicket> OrderTickets => Set<OrderTicket>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<OrderVoucher> OrderVouchers => Set<OrderVoucher>();
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Constants.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketingDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
