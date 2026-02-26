using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configs
{
       public class ExternalIdentityConfiguration : IEntityTypeConfiguration<ExternalIdentity>
       {
              public void Configure(EntityTypeBuilder<ExternalIdentity> builder)
              {
                     builder.ToTable("external_identity");

                     // --- Primary Key ---
                     builder.HasKey(ei => ei.Id);

                     builder.Property(ei => ei.Id)
                            .HasColumnName("id")
                            .HasColumnType("uuid")
                            .HasDefaultValueSql("gen_random_uuid()")
                            .ValueGeneratedNever();

                     // --- Properties ---
                     builder.Property(ei => ei.UserId)
                            .HasColumnName("user_id")
                            .HasColumnType("uuid")
                            .IsRequired();

                     builder.Property(ei => ei.Provider)
                            .HasColumnName("provider")
                            .HasMaxLength(100)
                            .IsRequired();

                     builder.Property(ei => ei.ProviderKey)
                            .HasColumnName("provider_key")
                            .HasMaxLength(255)
                            .IsRequired();

                     builder.Property(ei => ei.LinkedAt)
                            .HasColumnName("linked_at")
                            .HasColumnType("timestamp with time zone")
                            .HasDefaultValueSql("NOW()");

                     // --- Auditing (from Entity<Guid>) ---
                     builder.Property(ei => ei.CreatedAt)
                            .HasColumnName("created_at")
                            .HasColumnType("timestamp with time zone")
                            .HasDefaultValueSql("NOW()");

                     builder.Property(ei => ei.CreatedBy)
                            .HasColumnName("created_by")
                            .HasMaxLength(100);

                     builder.Property(ei => ei.ModifiedAt)
                            .HasColumnName("modified_at")
                            .HasColumnType("timestamp with time zone");

                     builder.Property(ei => ei.ModifiedBy)
                            .HasColumnName("modified_by")
                            .HasMaxLength(100);

                     builder.Property(ei => ei.IsActive)
                            .HasColumnName("is_active")
                            .HasDefaultValue(true);

                     // --- Relationships ---
                     builder.HasOne<User>()
                            .WithMany(u => u.ExternalIdentities)
                            .HasForeignKey(ei => ei.UserId)
                            .OnDelete(DeleteBehavior.Cascade);

                     // --- Indexes ---
                     builder.HasIndex(ei => new { ei.Provider, ei.ProviderKey })
                            .IsUnique()
                            .HasDatabaseName("ix_external_identity_provider_key");

                     builder.HasIndex(ei => ei.UserId)
                            .HasDatabaseName("ix_external_identity_user_id");

                     builder.HasIndex(ei => ei.CreatedAt)
                            .HasDatabaseName("ix_external_identity_created_at");
              }
       }
}
