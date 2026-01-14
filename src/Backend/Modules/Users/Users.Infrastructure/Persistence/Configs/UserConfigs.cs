using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

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

            builder.Property(u => u.PhoneNumber)
                   .HasColumnName("phone_number")
                   .HasMaxLength(20);

            builder.Property(u => u.Address)
                   .HasColumnName("address")
                   .HasMaxLength(255);

            builder.Property(u => u.ProfileImageUrl)
                   .HasColumnName("profile_image_url")
                   .HasMaxLength(500);

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

            // 1. Roles (Many-to-Many)
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

            // IMPORTANT: Tell EF Core to access the Roles collection via the backing field
            // because the property has a private setter.
            builder.Navigation(u => u.Roles)
                   .UsePropertyAccessMode(PropertyAccessMode.Field);

            // 2. Refresh Tokens (One-to-Many)
           
            builder.HasMany(u => u.RefreshTokens)
                   .WithOne()
                   .HasForeignKey("UserId"); // Explicitly linking to the Shadow Property or Property in RefreshToken

            builder.Navigation(u => u.RefreshTokens)
                   .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}