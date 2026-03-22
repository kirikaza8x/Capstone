using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;
using Shared.Infrastructure.Outbox;

namespace Payments.Infrastructure.Persistence.Contexts
{
    public class PaymentModuleDbContext : DbContext
    {
        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
        public DbSet<Wallet> Wallets { get; set; } = null!;
        public DbSet<WalletTransaction> WalletTransactions { get; set; } = null!;
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; } = null!;
        public DbSet<BatchPaymentItem> BatchPaymentItems { get; set; } = null!;
        public DbSet<RefundRequest> RefundRequests { get; set; } = null!;

        public PaymentModuleDbContext(DbContextOptions<PaymentModuleDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Constants.SchemaName);

            // Apply all IEntityTypeConfiguration<T> from assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentModuleDbContext).Assembly);

            // Outbox
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
