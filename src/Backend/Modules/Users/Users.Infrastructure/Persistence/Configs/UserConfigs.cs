using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Users.Infrastructure.Persistence.Configs
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("user");

            // --- Primary Key ---
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()");

            // --- Properties ---
            builder.Property(u => u.Email)
                   .HasColumnName("email")
                   .HasMaxLength(255);

            builder.Property(u => u.UserName)
                   .HasColumnName("username")
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                   .HasColumnName("password_hash")
                   .IsRequired();

            builder.Property(u => u.FirstName)
                   .HasColumnName("first_name")
                   .HasMaxLength(100);

            builder.Property(u => u.LastName)
                   .HasColumnName("last_name")
                   .HasMaxLength(100);

            builder.Property(u => u.Birthday)
                   .HasColumnName("birthday");

            builder.Property(u => u.Gender)
                   .HasColumnName("gender")
                   .HasConversion<string>()
                   .HasMaxLength(10);

            builder.Property(u => u.PhoneNumber)
                   .HasColumnName("phone_number")
                   .HasMaxLength(20);

            builder.Property(u => u.Address)
                   .HasColumnName("address")
                   .HasMaxLength(255);

            builder.Property(u => u.Description)
                   .HasColumnName("description")
                   .HasMaxLength(1000);

            builder.Property(u => u.SocialLink)
                   .HasColumnName("social_link")
                   .HasMaxLength(500);

            builder.Property(u => u.ProfileImageUrl)
                   .HasColumnName("profile_image_url")
                   .HasMaxLength(500);

            builder.Property(u => u.Status)
                   .HasColumnName("status")
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .HasDefaultValue(UserStatus.Active);

            // --- Wallet Relationship ---
            builder.HasOne(u => u.Wallet)
                   .WithOne(w => w.User)
                   .HasForeignKey<Wallet>(w => w.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // --- Indexes ---
            builder.HasIndex(u => u.Email)
                   .IsUnique()
                   .HasDatabaseName("ix_user_email");

            builder.HasIndex(u => u.UserName)
                   .IsUnique()
                   .HasDatabaseName("ix_user_username");

            builder.HasIndex(u => u.Status)
                   .HasDatabaseName("ix_user_status");

            builder.HasIndex(u => u.IsActive)
                   .HasDatabaseName("ix_user_is_active");

            builder.HasIndex(u => u.CreatedAt)
                   .HasDatabaseName("ix_user_created_at");

            // --- Auditing (from AggregateRoot) ---
            builder.Property(u => u.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("NOW()");

            builder.Property(u => u.CreatedBy)
                   .HasColumnName("created_by")
                   .HasMaxLength(100);

            builder.Property(u => u.ModifiedAt)
                   .HasColumnName("modified_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(u => u.ModifiedBy)
                   .HasColumnName("modified_by")
                   .HasMaxLength(100);

            builder.Property(u => u.IsActive)
                   .HasColumnName("is_active")
                   .HasDefaultValue(true);

            // --- Relationships ---

            // Roles (Many-to-Many)
            builder.HasMany(u => u.Roles)
                   .WithMany()
                   .UsingEntity<Dictionary<string, object>>(
                       "user_roles",
                       j => j
                           .HasOne<Role>()
                           .WithMany()
                           .HasForeignKey("role_id")
                           .HasConstraintName("fk_userroles_role")
                           .OnDelete(DeleteBehavior.Cascade),
                       j => j
                           .HasOne<User>()
                           .WithMany()
                           .HasForeignKey("user_id")
                           .HasConstraintName("fk_userroles_user")
                           .OnDelete(DeleteBehavior.Cascade),
                       j =>
                       {
                           j.HasKey("user_id", "role_id");
                           j.ToTable("user_roles");
                       });

            builder.Navigation(u => u.Roles)
                   .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Navigation(u => u.RefreshTokens)
                   .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
