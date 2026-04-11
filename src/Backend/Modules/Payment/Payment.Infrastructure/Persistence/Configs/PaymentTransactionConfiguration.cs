using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Enums;
using Payments.Domain.Entities;
using Shared.Infrastructure.Extensions;

namespace Payments.Infrastructure.Persistence.Configs;

public class PaymentTransactionConfiguration
    : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("payment_transaction");

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

        builder.Property(t => t.WalletId)
            .HasColumnName("wallet_id");

        builder.Property(t => t.OrderId)
            .HasColumnName("order_id");

        builder.Property(t => t.EventId)
            .HasColumnName("event_id");

        builder.Property(t => t.ReferenceId)
            .HasColumnName("reference_id");

        builder.Property(t => t.ReferenceType)
            .HasColumnName("reference_type")
            .HasConversion(
                v => v.HasValue ? ToSnakeCase(v.Value) : null,
                v => string.IsNullOrWhiteSpace(v) ? null : FromSnakeCase(v))
            .HasMaxLength(20);

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

        // --- Core ---
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

        // --- Items relationship ---
        builder.HasMany(t => t.Items)
            .WithOne()
            .HasForeignKey(i => i.PaymentTransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Indexes ---
        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("ix_payment_transaction_user_id");

        builder.HasIndex(t => t.WalletId)
            .HasDatabaseName("ix_payment_transaction_wallet_id");

        builder.HasIndex(t => t.OrderId)
            .HasDatabaseName("ix_payment_transaction_order_id");

        builder.HasIndex(t => t.ReferenceId)
            .HasDatabaseName("ix_payment_transaction_reference_id");

        builder.HasIndex(t => t.ReferenceType)
            .HasDatabaseName("ix_payment_transaction_reference_type");

        builder.HasIndex(t => t.InternalStatus)
            .HasDatabaseName("ix_payment_transaction_internal_status");

        builder.HasIndex(t => t.GatewayTxnRef)
            .HasDatabaseName("ix_payment_transaction_txn_ref");

        builder.HasIndex(t => t.GatewayResponseCode)
            .HasDatabaseName("ix_payment_transaction_response_code");

        builder.HasIndex(t => new { t.UserId, t.InternalStatus })
            .HasDatabaseName("ix_payment_transaction_user_status");
    }

    private static string ToSnakeCase(PaymentReferenceType referenceType) => referenceType switch
    {
        PaymentReferenceType.TicketOrder => "ticket_order",
        PaymentReferenceType.AiPackage => "ai_package",
        _ => throw new ArgumentOutOfRangeException(nameof(referenceType), referenceType, null)
    };

    private static PaymentReferenceType FromSnakeCase(string value) => value.ToLowerInvariant() switch
    {
        "ticket_order" => PaymentReferenceType.TicketOrder,
        "ai_package" => PaymentReferenceType.AiPackage,
        _ => throw new InvalidOperationException($"Unknown reference_type value: '{value}'")
    };
}
