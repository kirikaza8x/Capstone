using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Enums;
using Payments.Domain.Entities;
using Shared.Infrastructure.Extensions;

namespace Payments.Infrastructure.Persistence.Configs;

public class BatchPaymentItemConfiguration : IEntityTypeConfiguration<BatchPaymentItem>
{
    public void Configure(EntityTypeBuilder<BatchPaymentItem> builder)
    {
        builder.ToTable("batch_payment_item");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasColumnName("id")
               .HasColumnType("uuid")
               .ValueGeneratedNever();

        builder.Property(x => x.PaymentTransactionId)
               .HasColumnName("payment_transaction_id")
               .IsRequired();

        builder.Property(x => x.EventId)
               .HasColumnName("event_id")
               .IsRequired();

        builder.Property(x => x.Amount)
               .HasColumnName("amount")
               .HasColumnType("numeric(18,2)")
               .IsRequired();

        builder.Property(x => x.InternalStatus)
               .HasColumnName("internal_status")
               .HasConversion<string>()
               .HasMaxLength(30)
               .IsRequired()
               .HasDefaultValue(PaymentInternalStatus.AwaitingGateway);

        builder.Property(x => x.RefundedAt)
               .HasColumnName("refunded_at")
               .HasColumnType("timestamp with time zone");

        builder.ConfigureAudit<BatchPaymentItem, Guid>();

        builder.HasIndex(x => x.PaymentTransactionId)
               .HasDatabaseName("ix_batch_payment_item_transaction_id");

        builder.HasIndex(x => x.EventId)
               .HasDatabaseName("ix_batch_payment_item_event_id");

        builder.HasIndex(x => new { x.EventId, x.InternalStatus })
               .HasDatabaseName("ix_batch_payment_item_event_status");
    }
}