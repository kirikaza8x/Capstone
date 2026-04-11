using AI.Domain.Entities;
using AI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI.Infrastructure.Persistence.Configs;

public class AiPackageConfiguration : IEntityTypeConfiguration<AiPackage>
{
    public void Configure(EntityTypeBuilder<AiPackage> builder)
    {
        builder.ToTable("ai_package");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<AiPackageType>(v, true))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasColumnName("price")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.TokenQuota)
            .HasColumnName("token_quota")
            .IsRequired();

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

        builder.HasIndex(x => x.Name).HasDatabaseName("ix_ai_package_name");
        builder.HasIndex(x => x.Type).HasDatabaseName("ix_ai_package_type");
    }
}
