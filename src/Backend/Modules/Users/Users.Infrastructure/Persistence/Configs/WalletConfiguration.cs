using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configs
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("wallet");

            // --- Primary Key ---
            builder.HasKey(w => w.Id);

            builder.Property(w => w.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .ValueGeneratedNever()
                   .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(w => w.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(w => w.Balance)
                   .HasColumnName("balance")
                   .HasColumnType("numeric(18,2)")
                   .IsRequired();

            builder.Property(w => w.Status)
                   .HasColumnName("status")
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            // --- Indexes ---
            builder.HasIndex(w => w.UserId)
                   .IsUnique()
                   .HasDatabaseName("ix_wallet_user_id");

            builder.HasIndex(w => w.Status)
                   .HasDatabaseName("ix_wallet_status");

            // --- Relationships ---
            builder.HasMany(w => w.Transactions)
                   .WithOne()
                   .HasForeignKey(t => t.WalletId)
                   .OnDelete(DeleteBehavior.Cascade);

            // --- Auditing (Entity<Guid>) ---
            builder.Property(w => w.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(w => w.CreatedBy)
                   .HasColumnName("created_by")
                   .HasMaxLength(100);

            builder.Property(w => w.ModifiedAt)
                   .HasColumnName("modified_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(w => w.ModifiedBy)
                   .HasColumnName("modified_by")
                   .HasMaxLength(100);

            builder.Property(w => w.IsActive)
                   .HasColumnName("is_active");
        }
    }
}
