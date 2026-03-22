using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Shared.Infrastructure.Extensions;

namespace Payments.Infrastructure.Persistence.Configs;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("wallet_transaction");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
               .HasColumnName("id")
               .HasColumnType("uuid")
               .ValueGeneratedNever();

        builder.Property(t => t.WalletId)
               .HasColumnName("wallet_id")
               .IsRequired();

        builder.Property(t => t.Type)
               .HasColumnName("type")
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(t => t.Direction)
               .HasColumnName("direction")
               .HasConversion<string>()
               .HasMaxLength(10)
               .IsRequired();

        builder.Property(t => t.Amount)
               .HasColumnName("amount")
               .HasColumnType("numeric(18,2)")
               .IsRequired();

        builder.Property(t => t.BalanceBefore)
               .HasColumnName("balance_before")
               .HasColumnType("numeric(18,2)")
               .IsRequired();

        builder.Property(t => t.BalanceAfter)
               .HasColumnName("balance_after")
               .HasColumnType("numeric(18,2)")
               .IsRequired();

        builder.Property(t => t.Note)
               .HasColumnName("note")
               .HasMaxLength(500);

        builder.Property(t => t.Status)
               .HasColumnName("status")
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired()
               .HasDefaultValue(TransactionStatus.Pending);

        builder.ConfigureAudit<WalletTransaction, Guid>();

        builder.HasIndex(t => t.WalletId)
               .HasDatabaseName("ix_wallet_transaction_wallet_id");

        builder.HasIndex(t => new { t.WalletId, t.Type })
               .HasDatabaseName("ix_wallet_transaction_wallet_type");
    }
}