using Events.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Outbox;

namespace Events.Infrastructure.Data;

public sealed class EventsDbContext(DbContextOptions<EventsDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventSession> EventSessions => Set<EventSession>();
    public DbSet<TicketType> TicketTypes => Set<TicketType>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Category> EventCategories => Set<Category>();
    public DbSet<EventCategory> EventCategoryMappings => Set<EventCategory>();
    public DbSet<EventImage> EventImages => Set<EventImage>();
    public DbSet<EventActorImage> EventActorImages => Set<EventActorImage>();
    public DbSet<EventMember> EventMembers => Set<EventMember>();
    public DbSet<Hashtag> Hashtags => Set<Hashtag>();
    public DbSet<EventHashtag> EventHashtags => Set<EventHashtag>();
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Constants.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventsDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
