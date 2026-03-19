
using Shared.Domain.DDD;

namespace Events.Domain.Entities;

public sealed class EventMember : Entity<Guid>
{
    private EventMember() { }

    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public List<string> Permissions { get; private set; } = [];
    public string Status { get; private set; } = string.Empty;
    public Guid AssignedBy { get; private set; }

    public Event Event { get; private set; } = null!;

    public static EventMember Create(Guid eventId, Guid userId, List<string> permissions, Guid assignedBy)
    {
        return new EventMember
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Permissions = permissions,
            Status = "Active",
            AssignedBy = assignedBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePermissions(List<string> permissions)
    {
        Permissions = permissions;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = "Inactive";
        ModifiedAt = DateTime.UtcNow;
    }
}
