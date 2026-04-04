using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Enums;
using Payments.Domain.Entities;
using Shared.Infrastructure.Extensions;

namespace Payments.Infrastructure.Persistence.Configs;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("wallet");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
               .HasColumnName("id")
               .HasColumnType("uuid")
               .ValueGeneratedNever();

        builder.Property(w => w.UserId)
               .HasColumnName("user_id")
               .IsRequired();

        builder.Property(w => w.Balance)
               .HasColumnName("balance")
               .HasColumnType("numeric(18,2)")
               .IsRequired()
               .HasDefaultValue(0m);

        builder.Property(w => w.Status)
               .HasColumnName("status")
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired()
               .HasDefaultValue(WalletStatus.Active);

        builder.ConfigureAudit<Wallet, Guid>();

        builder.HasMany(w => w.Transactions)
               .WithOne()
               .HasForeignKey(t => t.WalletId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.UserId)
               .IsUnique()
               .HasDatabaseName("ix_wallet_user_id");

        builder.HasIndex(w => w.Status)
               .HasDatabaseName("ix_wallet_status");
    }
}
