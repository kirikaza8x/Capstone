using Events.Domain.Enums;
using Shared.Domain.DDD;

namespace Events.Domain.Entities;

public sealed class EventCategory : Entity<int>
{
    private EventCategory() { }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public static EventCategory Create(string code, string name, string? description)
    {
        return new EventCategory
        {
            Code = code,
            Name = name,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        ModifiedAt = DateTime.UtcNow;
    }
}