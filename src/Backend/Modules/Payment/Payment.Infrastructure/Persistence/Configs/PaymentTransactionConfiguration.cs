using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Enums;
using Payments.Domain.Entities;
using Shared.Infrastructure.Extensions;

namespace Payments.Infrastructure.Persistence.Configs
{
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("payment_transaction");

            // --- Primary Key ---
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .ValueGeneratedNever()
                   .HasDefaultValueSql("gen_random_uuid()");

            // --- References ---
            builder.Property(t => t.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(t => t.EventId)
                   .HasColumnName("event_id");

            builder.Property(t => t.WalletId)
                   .HasColumnName("wallet_id");

            builder.Property(t => t.Type)
                   .HasColumnName("type")
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            // --- Internal lifecycle ---
            builder.Property(t => t.InternalStatus)
                   .HasColumnName("internal_status")
                   .HasConversion<string>()
                   .HasMaxLength(30)
                   .IsRequired()
                   .HasDefaultValue(PaymentInternalStatus.AwaitingGateway);

            // --- Core transaction info ---
            builder.Property(t => t.Amount)
                   .HasColumnName("amount")
                   .HasColumnType("numeric(18,2)")
                   .IsRequired();

            builder.Property(t => t.Currency)
                   .HasColumnName("currency")
                   .HasMaxLength(10)
                   .IsRequired();

            // --- Gateway fields ---
            builder.Property(t => t.GatewayTransactionNo)
                   .HasColumnName("gateway_transaction_no")
                   .HasMaxLength(100);

            builder.Property(t => t.GatewayResponseCode)
                   .HasColumnName("gateway_response_code")
                   .HasMaxLength(10);

            builder.Property(t => t.GatewayStatus)
                   .HasColumnName("gateway_status")
                   .HasMaxLength(50);

            builder.Property(t => t.GatewayOrderInfo)
                   .HasColumnName("gateway_order_info")
                   .HasMaxLength(500);

            builder.Property(t => t.GatewayTxnRef)
                   .HasColumnName("gateway_txn_ref")
                   .HasMaxLength(100);

            builder.Property(t => t.GatewayBankCode)
                   .HasColumnName("gateway_bank_code")
                   .HasMaxLength(50);

            builder.Property(t => t.GatewayBankTranNo)
                   .HasColumnName("gateway_bank_tran_no")
                   .HasMaxLength(100);

            builder.Property(t => t.GatewayCardType)
                   .HasColumnName("gateway_card_type")
                   .HasMaxLength(50);

            builder.Property(t => t.GatewayPayDate)
                   .HasColumnName("gateway_pay_date")
                   .HasMaxLength(20);

            builder.Property(t => t.GatewayTmnCode)
                   .HasColumnName("gateway_tmn_code")
                   .HasMaxLength(50);

            builder.Property(t => t.GatewaySecureHash)
                   .HasColumnName("gateway_secure_hash")
                   .HasMaxLength(200);

            builder.Property(t => t.GatewaySecureHashType)
                   .HasColumnName("gateway_secure_hash_type")
                   .HasMaxLength(50);

            builder.Property(t => t.GatewayLocale)
                   .HasColumnName("gateway_locale")
                   .HasMaxLength(10);

            builder.Property(t => t.GatewayIpAddr)
                   .HasColumnName("gateway_ip_addr")
                   .HasMaxLength(50);

            builder.Property(t => t.GatewayCreateDate)
                   .HasColumnName("gateway_create_date")
                   .HasMaxLength(20);

            builder.Property(t => t.GatewayOrderType)
                   .HasColumnName("gateway_order_type")
                   .HasMaxLength(50);

            builder.Property(t => t.GatewayMerchant)
                   .HasColumnName("gateway_merchant")
                   .HasMaxLength(100);

            // --- Lifecycle timestamps ---
            builder.Property(t => t.CompletedAt)
                   .HasColumnName("completed_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(t => t.FailedAt)
                   .HasColumnName("failed_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(t => t.RefundedAt)
                   .HasColumnName("refunded_at")
                   .HasColumnType("timestamp with time zone");

            // --- Auditing ---
            builder.ConfigureAudit<PaymentTransaction, Guid>();

            // --- Indexes ---
            builder.HasIndex(t => t.UserId)
                   .HasDatabaseName("ix_payment_transaction_user_id");

            builder.HasIndex(t => t.EventId)
                   .HasDatabaseName("ix_payment_transaction_event_id");

            builder.HasIndex(t => t.WalletId)
                   .HasDatabaseName("ix_payment_transaction_wallet_id");

            builder.HasIndex(t => t.InternalStatus)
                   .HasDatabaseName("ix_payment_transaction_internal_status");

            builder.HasIndex(t => t.GatewayTxnRef)
                   .IsUnique()
                   .HasDatabaseName("ix_payment_transaction_txn_ref");

            builder.HasIndex(t => t.GatewayResponseCode)
                   .HasDatabaseName("ix_payment_transaction_response_code");

            builder.HasIndex(t => t.GatewayStatus)
                   .HasDatabaseName("ix_payment_transaction_gateway_status");

            builder.HasIndex(t => new { t.EventId, t.InternalStatus })
                   .HasDatabaseName("ix_payment_transaction_event_status");
        }
    }
}
