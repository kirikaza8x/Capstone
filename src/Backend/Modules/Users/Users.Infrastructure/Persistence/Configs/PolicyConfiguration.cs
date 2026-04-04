using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configs
{
    public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
    {
        public void Configure(EntityTypeBuilder<Policy> builder)
        {
            builder.ToTable("policy");

            builder.HasKey(x => x.Id);

            builder.Property(r => r.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .ValueGeneratedNever();

            builder.Property(x => x.Type)
                .HasColumnName("type")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.FileUrl)
                .HasColumnName("file_url")
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(x => x.CreatedAt).HasColumnName("created_at");
            builder.Property(x => x.CreatedBy).HasColumnName("created_by");
            builder.Property(x => x.ModifiedAt).HasColumnName("modified_at");
            builder.Property(x => x.ModifiedBy).HasColumnName("modified_by");
            builder.Property(x => x.IsActive).HasColumnName("is_active");

            builder.HasIndex(x => x.Type).HasDatabaseName("ix_policy_type");
        }
    }
}
