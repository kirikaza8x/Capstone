using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.DDD;

namespace Shared.Infrastructure.Extensions
{
    public static class AuditConfigurationExtensions
    {
        public static void ConfigureAudit<TEntity, TId>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : Entity<TId>
        {
            builder.Property(e => e.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(e => e.CreatedBy)
                   .HasColumnName("created_by")
                   .HasMaxLength(100);

            builder.Property(e => e.ModifiedAt)
                   .HasColumnName("modified_at")
                   .HasColumnType("timestamp with time zone");

            builder.Property(e => e.ModifiedBy)
                   .HasColumnName("modified_by")
                   .HasMaxLength(100);

            builder.Property(e => e.IsActive)
                   .HasColumnName("is_active");
        }
    }
}
