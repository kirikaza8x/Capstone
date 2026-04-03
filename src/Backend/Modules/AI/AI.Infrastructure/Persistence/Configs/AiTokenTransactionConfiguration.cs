using AI.Domain.Entities;
using AI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI.Infrastructure.Persistence.Configs;

public class AiTokenTransactionConfiguration : IEntityTypeConfiguration<AiTokenTransaction>
{
    public void Configure(EntityTypeBuilder<AiTokenTransaction> builder)
    {
        builder.ToTable("ai_token_transaction");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.QuotaId)
            .HasColumnName("quota_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.PackageId)
            .HasColumnName("package_id")
            .HasColumnType("uuid")
            .IsRequired(false);

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<AiTokenTransactionType>(v, true))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .IsRequired();

        builder.Property(x => x.BalanceAfter)
            .HasColumnName("balance_after")
            .IsRequired();

        builder.Property(x => x.ReferenceId)
            .HasColumnName("reference_id")
            .HasColumnType("uuid")
            .IsRequired(false);

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(x => x.ModifiedAt)
            .HasColumnName("modified_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.ModifiedBy)
            .HasColumnName("modified_by")
            .HasMaxLength(100);

        builder.HasOne(x => x.Quota)
            .WithMany()
            .HasForeignKey(x => x.QuotaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Package)
            .WithMany()
            .HasForeignKey(x => x.PackageId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.QuotaId).HasDatabaseName("ix_ai_token_transaction_quota_id");
        builder.HasIndex(x => x.PackageId).HasDatabaseName("ix_ai_token_transaction_package_id");
        builder.HasIndex(x => x.Type).HasDatabaseName("ix_ai_token_transaction_type");
        builder.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_ai_token_transaction_created_at");
    }
}
