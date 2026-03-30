using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Shared.Infrastructure.Extensions;

namespace Payments.Infrastructure.Persistence.Configs;

public class WithdrawalRequestConfiguration : IEntityTypeConfiguration<WithdrawalRequest>
{
    public void Configure(EntityTypeBuilder<WithdrawalRequest> builder)
    {
        builder.ToTable("withdrawal_request");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
               .HasColumnName("id")
               .HasColumnType("uuid")
               .ValueGeneratedNever();

        builder.Property(w => w.UserId)
               .HasColumnName("user_id")
               .HasColumnType("uuid")
               .IsRequired();

        builder.Property(w => w.WalletId)
               .HasColumnName("wallet_id")
               .HasColumnType("uuid")
               .IsRequired();

        builder.Property(w => w.BankAccountNumber)
               .HasColumnName("bank_account_number")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(w => w.BankName)
               .HasColumnName("bank_name")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(w => w.Amount)
               .HasColumnName("amount")
               .HasColumnType("numeric(18,2)")
               .IsRequired();

        builder.Property(w => w.Notes)
               .HasColumnName("notes")
               .HasMaxLength(500);

        builder.Property(w => w.Status)
               .HasColumnName("status")
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired()
               .HasDefaultValue(WithdrawalRequestStatus.Pending);

        builder.Property(w => w.AdminNote)
               .HasColumnName("admin_note")
               .HasMaxLength(500);

        builder.Property(w => w.ProcessedAt)
               .HasColumnName("processed_at")
               .HasColumnType("timestamp with time zone");

        builder.Property(w => w.WalletTransactionId)
               .HasColumnName("wallet_transaction_id")
               .HasColumnType("uuid");

        builder.ConfigureAudit<WithdrawalRequest, Guid>();

        builder.HasOne<Wallet>()
               .WithMany()
               .HasForeignKey(w => w.WalletId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(w => new { w.UserId, w.Status })
               .HasDatabaseName("ix_withdrawal_request_user_id_status");

        builder.HasIndex(w => w.WalletId)
               .HasDatabaseName("ix_withdrawal_request_wallet_id");

        builder.HasIndex(w => w.WalletTransactionId)
               .HasDatabaseName("ix_withdrawal_request_wallet_transaction_id");
    }
}