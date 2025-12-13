using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ConfigsDB.Domain.Entities;

namespace ConfigsDB.Infrastructure.Persistence.Configurations
{
    public class ConfigSettingDBSettingConfigs : IEntityTypeConfiguration<ConfigSetting>
    {
        public void Configure(EntityTypeBuilder<ConfigSetting> builder)
        {
            builder.ToTable("config_setting");

            // Primary key
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()");

            // Properties
            builder.Property(c => c.Key)
                   .HasColumnName("key")
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(c => c.Value)
                   .HasColumnName("value")
                   .IsRequired();

            builder.Property(c => c.Category)
                   .HasColumnName("category")
                   .HasMaxLength(100);

            builder.Property(c => c.Environment)
                   .HasColumnName("environment")
                   .HasMaxLength(50)
                   .HasDefaultValue("Global");

            builder.Property(c => c.Description)
                   .HasColumnName("description")
                   .HasMaxLength(500);

            builder.Property(c => c.IsActive)
                   .HasColumnName("is_active")
                   .HasDefaultValue(true);

            builder.Property(c => c.IsEncrypted)
                   .HasColumnName("is_encrypted")
                   .HasDefaultValue(false);

            // Auditing fields 
            builder.Property(c => c.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("NOW()");

            builder.Property(c => c.CreatedBy)
                   .HasColumnName("created_by")
                   .HasMaxLength(100);

            builder.Property(c => c.ModifiedAt)
                   .HasColumnName("modified_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(c => c.ModifiedBy)
                   .HasColumnName("modified_by")
                   .HasMaxLength(100);

            builder.Property(c => c.IsDeleted)
                   .HasColumnName("is_deleted")
                   .HasDefaultValue(false);
        }
    }
}
