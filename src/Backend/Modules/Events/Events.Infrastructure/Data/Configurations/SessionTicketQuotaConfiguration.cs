using Events.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Data.Configurations;

internal sealed class SessionTicketQuotaConfiguration : IEntityTypeConfiguration<SessionTicketQuota>
{
    public void Configure(EntityTypeBuilder<SessionTicketQuota> builder)
    {
        builder.ToTable("session_ticket_quotas");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Quantity).IsRequired();

        builder.HasIndex(e => new { e.EventSessionId, e.TicketTypeId })
            .IsUnique()
            .HasDatabaseName("ix_session_ticket_quota_session_tickettype");

        builder.HasOne(e => e.EventSession)
            .WithMany()
            .HasForeignKey(e => e.EventSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.TicketType)
            .WithMany()
            .HasForeignKey(e => e.TicketTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}