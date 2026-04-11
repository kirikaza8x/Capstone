using Events.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Data.Configurations;

internal sealed class HashtagConfiguration : IEntityTypeConfiguration<Hashtag>
{
    public void Configure(EntityTypeBuilder<Hashtag> builder)
    {
        builder.ToTable("hashtags");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("hashtag_id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Slug)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.UsageCount)
            .HasDefaultValue(0);

        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => e.Slug).IsUnique();
    }
}
