using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configs
{
    public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
    {
        public void Configure(EntityTypeBuilder<UserSession> builder)
        {
            builder.ToTable("user_session");

            // --- Primary Key ---
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid");

            builder.Property(s => s.SessionId)
                   .HasColumnName("session_id")
                   .HasColumnType("uuid")
                   .IsRequired();

            builder.Property(s => s.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(s => s.DeviceType)
                   .HasColumnName("device_type")
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(s => s.Source)
                   .HasColumnName("source")
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(s => s.CampaignId)
                   .HasColumnName("campaign_id");

            builder.Property(s => s.LastActiveAt)
                   .HasColumnName("last_active_at")
                   .HasColumnType("timestamp with time zone");

            // --- Indexes ---
            builder.HasIndex(s => s.SessionId)
                   .IsUnique()
                   .HasDatabaseName("ix_user_session_session_id");

            builder.HasIndex(s => s.UserId)
                   .HasDatabaseName("ix_user_session_user_id");

            builder.HasIndex(s => s.LastActiveAt)
                   .HasDatabaseName("ix_user_session_last_active_at");

            builder.HasIndex(s => s.CampaignId)
                   .HasDatabaseName("ix_user_session_campaign_id");

            // --- Auditing ---
            builder.Property(s => s.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(s => s.CreatedBy)
                   .HasColumnName("created_by")
                   .HasMaxLength(100);

            builder.Property(s => s.ModifiedAt)
                   .HasColumnName("modified_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(s => s.ModifiedBy)
                   .HasColumnName("modified_by")
                   .HasMaxLength(100);

            builder.Property(s => s.IsActive)
                   .HasColumnName("is_active");
        }
    }
}
