using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Shared.Infrastructure.Extensions;

namespace Payments.Infrastructure.Persistence.Configs;

public class RefundRequestConfiguration
    : IEntityTypeConfiguration<RefundRequest>
{
    public void Configure(EntityTypeBuilder<RefundRequest> builder)
    {
        builder.ToTable("refund_request");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasColumnName("id")
               .HasColumnType("uuid")
               .ValueGeneratedNever();

        builder.Property(x => x.UserId)
               .HasColumnName("user_id")
               .IsRequired();

        builder.Property(x => x.PaymentTransactionId)
               .HasColumnName("payment_transaction_id")
               .IsRequired();

        builder.Property(x => x.EventSessionId)
               .HasColumnName("event_session_id");

        builder.Property(x => x.Scope)
               .HasColumnName("scope")
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.Status)
               .HasColumnName("status")
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired()
               .HasDefaultValue(RefundRequestStatus.Pending);

        builder.Property(x => x.RequestedAmount)
               .HasColumnName("requested_amount")
               .HasColumnType("numeric(18,2)")
               .IsRequired();

        builder.Property(x => x.UserReason)
               .HasColumnName("user_reason")
               .HasMaxLength(1000)
               .IsRequired();

        builder.Property(x => x.ReviewerNote)
               .HasColumnName("reviewer_note")
               .HasMaxLength(1000);

        builder.Property(x => x.ReviewedByAdminId)
               .HasColumnName("reviewed_by_admin_id");

        builder.Property(x => x.ReviewedAt)
               .HasColumnName("reviewed_at")
               .HasColumnType("timestamp with time zone");

        builder.ConfigureAudit<RefundRequest, Guid>();

        builder.HasIndex(x => x.UserId)
               .HasDatabaseName("ix_refund_request_user_id");

        builder.HasIndex(x => x.PaymentTransactionId)
               .HasDatabaseName("ix_refund_request_transaction_id");

        builder.HasIndex(x => x.Status)
               .HasDatabaseName("ix_refund_request_status");

        builder.HasIndex(x => new { x.PaymentTransactionId, x.EventSessionId, x.Status })
               .HasDatabaseName("ix_refund_request_txn_session_status");
    }
}
