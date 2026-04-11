using Events.Domain.Enums; 
using Shared.Domain.Abstractions;
using Shared.Domain.DDD;

namespace Events.Domain.Entities;

public sealed class EventMember : Entity<Guid>
{
    private EventMember() { }

    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public List<string> Permissions { get; private set; } = [];
    public EventMemberStatus Status { get; private set; }
    public Guid AssignedBy { get; private set; }
    public Event Event { get; private set; } = null!;

    public static EventMember Create(
            Guid eventId,
            Guid userId,
            List<string> permissions,
            Guid assignedBy)
    {
        return new EventMember
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Permissions = permissions,
            Status = EventMemberStatus.Pending,
            AssignedBy = assignedBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public Result Confirm(DateTime? utcNow = null)
    {
        if (Status == EventMemberStatus.Active)
        {
            return Result.Success(); // Idempotent
        }

        if (Status != EventMemberStatus.Pending)
        {
            return Result.Failure(Error.Validation("EventMember.CannotConfirm", "Only pending invitations can be confirmed."));
        }

        Status = EventMemberStatus.Active;
        ModifiedAt = utcNow ?? DateTime.UtcNow;

        return Result.Success();
    }

    public void UpdatePermissions(List<string> permissions)
    {
        Permissions = permissions;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = EventMemberStatus.Inactive;
        ModifiedAt = DateTime.UtcNow;
    }
}
