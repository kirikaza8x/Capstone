using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Users.Infrastructure.Persistence.Configs
{
    public class OrganizerProfileConfiguration : IEntityTypeConfiguration<OrganizerProfile>
    {
        public void Configure(EntityTypeBuilder<OrganizerProfile> builder)
        {
            builder.ToTable("organizer_profile");

            // --- Primary Key ---
            builder.HasKey(op => op.Id);

            builder.Property(op => op.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()")
                   .ValueGeneratedNever();

            // --- Foreign Key ---
            builder.Property(op => op.UserId)
                   .HasColumnName("user_id")
                   .HasColumnType("uuid")
                   .IsRequired();

            builder.HasOne(op => op.User)
                   .WithMany(u => u.OrganizerProfiles)
                   .HasForeignKey(op => op.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // --- Versioning ---
            builder.Property(op => op.VersionNumber)
                   .HasColumnName("version_number")
                   .IsRequired();

            // --- Properties ---
            builder.Property(op => op.Logo)
                   .HasColumnName("logo")
                   .HasMaxLength(500);

            builder.Property(op => op.DisplayName)
                   .HasColumnName("display_name")
                   .HasMaxLength(255);

            builder.Property(op => op.AccountName)
                   .HasColumnName("account_name")
                   .HasMaxLength(255);

            builder.Property(op => op.AccountNumber)
                   .HasColumnName("account_number")
                   .HasMaxLength(50);

            builder.Property(op => op.BankCode)
                   .HasColumnName("bank_code")
                   .HasMaxLength(50);

            builder.Property(op => op.Branch)
                   .HasColumnName("branch")
                   .HasMaxLength(255);

            builder.Property(op => op.Description)
                   .HasColumnName("description")
                   .HasMaxLength(1000);

            builder.Property(op => op.BusinessType)
                   .HasColumnName("business_type")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(op => op.Address)
                   .HasColumnName("address")
                   .HasMaxLength(255);

            builder.Property(op => op.TaxCode)
                   .HasColumnName("tax_code")
                   .HasMaxLength(50);

            builder.Property(op => op.IdentityNumber)
                   .HasColumnName("identity_number")
                   .HasMaxLength(50);

            builder.Property(op => op.CompanyName)
                   .HasColumnName("company_name")
                   .HasMaxLength(255);

            builder.Property(op => op.SocialLink)
                   .HasColumnName("social_link")
                   .HasMaxLength(500);

            builder.Property(op => op.Status)
                   .HasColumnName("status")
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .HasDefaultValue(OrganizerStatus.Draft);

            builder.Property(op => op.VerifiedAt)
                   .HasColumnName("verified_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(op => op.Type)
                   .HasColumnName("type")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(op => op.RejectionReason)
                   .HasColumnName("reject_reason")
                   .HasColumnType("text")
                   .IsRequired(false);

            // --- Auditing ---
            builder.Property(op => op.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("NOW()");

            builder.Property(op => op.CreatedBy)
                   .HasColumnName("created_by")
                   .HasMaxLength(100);

            builder.Property(op => op.ModifiedAt)
                   .HasColumnName("modified_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(op => op.ModifiedBy)
                   .HasColumnName("modified_by")
                   .HasMaxLength(100);

            builder.Property(op => op.IsActive)
                   .HasColumnName("is_active")
                   .HasDefaultValue(true);

            // --- Indexes ---
            builder.HasIndex(op => op.DisplayName)
                   .HasDatabaseName("ix_organizerprofile_display_name");

            builder.HasIndex(op => op.Status)
                   .HasDatabaseName("ix_organizerprofile_status");

            builder.HasIndex(op => op.BusinessType)
                   .HasDatabaseName("ix_organizerprofile_business_type");

            builder.HasIndex(op => op.CreatedAt)
                   .HasDatabaseName("ix_organizerprofile_created_at");

            // --- Versioning Index (Important) ---
            builder.HasIndex(op => new { op.UserId, op.VersionNumber })
                   .HasDatabaseName("ix_organizerprofile_user_version");

            builder.HasIndex(op => new { op.UserId, op.Status })
                   .HasDatabaseName("ix_organizerprofile_user_status");
        }
    }
}
