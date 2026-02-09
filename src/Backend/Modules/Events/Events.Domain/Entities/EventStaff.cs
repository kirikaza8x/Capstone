using Shared.Domain.DDD;

namespace Events.Domain.Entities;

public sealed class EventStaff : Entity<Guid>
{
    private EventStaff() { }

    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public List<string> Permissions { get; private set; } = [];
    public string Status { get; private set; } = string.Empty;
    public Guid AssignedBy { get; private set; }

    public Event Event { get; private set; } = null!;

    public static EventStaff Create(
        Guid eventId,
        Guid userId,
        List<string> permissions,
        string status,
        Guid assignedBy)
    {
        return new EventStaff
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Permissions = permissions,
            Status = status,
            AssignedBy = assignedBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePermissions(List<string> permissions)
    {
        Permissions = permissions;
        ModifiedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(string status)
    {
        Status = status;
        ModifiedAt = DateTime.UtcNow;
    }
}