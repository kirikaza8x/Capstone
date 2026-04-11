using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configs
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_token");

            // Primary key
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()")
                   .ValueGeneratedNever()
                   ;

            // Properties
            builder.Property(rt => rt.Token)
                   .HasColumnName("token")
                   .IsRequired()
                   .HasMaxLength(512);

            builder.Property(rt => rt.ExpiryDate)
                   .HasColumnName("expiry_date")
                   .HasColumnType("timestamp with time zone")
                   .IsRequired();

            builder.Property(rt => rt.IsRevoked)
                   .HasColumnName("is_revoked")
                   .HasDefaultValue(false);

            builder.Property(rt => rt.UserId)
                   .HasColumnName("user_id")
                   .HasColumnType("uuid")
                   .IsRequired();

            builder.Property(rt => rt.DeviceId)
                   .HasColumnName("device_id")
                   .HasMaxLength(128);

            builder.Property(rt => rt.DeviceName)
                   .HasColumnName("device_name")
                   .HasMaxLength(128);

            builder.Property(rt => rt.IpAddress)
                   .HasColumnName("ip_address")
                   .HasMaxLength(64);

            builder.Property(rt => rt.UserAgent)
                   .HasColumnName("user_agent")
                   .HasMaxLength(512);

            // Auditing
            builder.Property(rt => rt.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("NOW()");

            builder.Property(rt => rt.CreatedBy)
                   .HasColumnName("created_by")
                   .HasMaxLength(100);

            builder.Property(rt => rt.ModifiedAt)
                   .HasColumnName("modified_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(rt => rt.ModifiedBy)
                   .HasColumnName("modified_by")
                   .HasMaxLength(100);

            builder.Property(rt => rt.IsActive)
                   .HasColumnName("is_active")
                   .HasDefaultValue(true);

            // Relationships (if you want to enforce FK to User)
            builder.HasOne<User>() // assuming you have a User entity
                   .WithMany(u => u.RefreshTokens)
                   .HasForeignKey(rt => rt.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
