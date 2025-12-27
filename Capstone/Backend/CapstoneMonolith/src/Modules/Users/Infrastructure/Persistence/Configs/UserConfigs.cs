using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configurations
{
       public class UserConfiguration : IEntityTypeConfiguration<User>
       {
              public void Configure(EntityTypeBuilder<User> builder)
              {
                     builder.ToTable("user");

                     // Primary key
                     builder.HasKey(u => u.Id);

                     builder.Property(u => u.Id)
                            .HasColumnName("id")
                            .HasColumnType("uuid")
                            .HasDefaultValueSql("gen_random_uuid()");

                     // Properties
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

                     builder.Property(u => u.RefreshToken)
                            .HasColumnName("refresh_token");

                     builder.Property(u => u.RefreshTokenExpiry)
                            .HasColumnName("refresh_token_expiry");

                     // Auditing
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

                     builder.Property(u => u.IsDeleted)
                                 .HasColumnName("is_deleted")
                                 .HasDefaultValue(false);

                     // Many-to-many Roles
                     builder
                         .HasMany(u => u.Roles)
                         .WithMany()
                         .UsingEntity<Dictionary<string, object>>(
                             "user_roles", // join table name
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
              }
       }
}
