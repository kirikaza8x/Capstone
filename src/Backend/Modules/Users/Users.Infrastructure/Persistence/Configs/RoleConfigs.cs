using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configs
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("role");

            // Primary key
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()");

            // Properties
            builder.Property(r => r.Name)
                   .HasColumnName("name")
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(r => r.Description)
                   .HasColumnName("description")
                   .HasMaxLength(255);

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
            builder.Property(u => u.IsActive)
                   .HasColumnName("is_active")
                   .HasDefaultValue(true);
        }
    }
}
