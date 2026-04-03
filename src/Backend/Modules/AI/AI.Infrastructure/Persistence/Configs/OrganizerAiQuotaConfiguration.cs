using AI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI.Infrastructure.Persistence.Configs;

public class OrganizerAiQuotaConfiguration : IEntityTypeConfiguration<OrganizerAiQuota>
{
    public void Configure(EntityTypeBuilder<OrganizerAiQuota> builder)
    {
        builder.ToTable("organizer_ai_quota");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.OrganizerId)
            .HasColumnName("organizer_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.SubscriptionTokens)
            .HasColumnName("subscription_tokens")
            .IsRequired();

        builder.Property(x => x.TopUpTokens)
            .HasColumnName("top_up_tokens")
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

        builder.HasIndex(x => x.OrganizerId)
            .IsUnique()
            .HasDatabaseName("ux_organizer_ai_quota_organizer");
    }
}
