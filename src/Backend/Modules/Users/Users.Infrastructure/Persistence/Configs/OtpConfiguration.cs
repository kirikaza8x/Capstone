using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configs
{
    public class OtpConfiguration : IEntityTypeConfiguration<Otp>
    {
        public void Configure(EntityTypeBuilder<Otp> builder)
        {
            builder.ToTable("otp");

            // --- Primary Key ---
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()");

            // --- Properties ---
            builder.Property(o => o.UserId)
                   .HasColumnName("user_id")
                   .HasColumnType("uuid")
                   .IsRequired();

            builder.Property(o => o.OtpCode)
                   .HasColumnName("otp_code")
                   .HasMaxLength(6)
                   .IsRequired();

            builder.Property(o => o.ExpiryDate)
                   .HasColumnName("expiry_date")
                   .HasColumnType("timestamp with time zone")
                   .IsRequired();

            builder.Property(o => o.IsUsed)
                   .HasColumnName("is_used")
                   .HasDefaultValue(false);

            builder.Property(o => o.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("NOW()");

            builder.Property(o => o.CreatedBy)
                   .HasColumnName("created_by")
                   .HasMaxLength(100);

            builder.Property(o => o.ModifiedAt)
                   .HasColumnName("modified_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(o => o.ModifiedBy)
                   .HasColumnName("modified_by")
                   .HasMaxLength(100);

            builder.Property(o => o.IsActive)
                   .HasColumnName("is_active")
                   .HasDefaultValue(true);

            // --- Relationships ---
            builder.HasOne<User>()
                   .WithMany(u => u.Otps) // assuming you add ICollection<Otp> Otps in User entity
                   .HasForeignKey(o => o.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // --- Indexes ---
            builder.HasIndex(o => new { o.UserId, o.OtpCode })
                   .HasDatabaseName("ix_otp_user_code");

            builder.HasIndex(o => o.ExpiryDate)
                   .HasDatabaseName("ix_otp_expiry_date");
        }
    }
}
