using Events.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure.Data;

public sealed class EventsDbContext(DbContextOptions<EventsDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventSession> EventSessions => Set<EventSession>();
    public DbSet<EventImage> EventImages => Set<EventImage>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<EventCategoryMapping> EventCategoryMappings => Set<EventCategoryMapping>();
    public DbSet<Hashtag> Hashtags => Set<Hashtag>();
    public DbSet<EventHashtag> EventHashtags => Set<EventHashtag>();
    public DbSet<EventStaff> EventStaffs => Set<EventStaff>();
    public DbSet<EventActorImage> EventActorImages => Set<EventActorImage>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Seat> Seats => Set<Seat>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Constants.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}