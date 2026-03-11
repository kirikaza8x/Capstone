using Shared.Domain.DDD;

namespace Events.Domain.Entities;

public sealed class Hashtag : Entity<int>
{
    private readonly List<EventHashtag> _eventHashtags = [];

    private Hashtag() { }

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public int UsageCount { get; private set; }

    public IReadOnlyCollection<EventHashtag> EventHashtags => _eventHashtags.AsReadOnly();

    public static Hashtag Create(string name, string slug)
    {
        return new Hashtag
        {
            Name = name,
            Slug = slug,
            UsageCount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string slug)
    {
        Name = name;
        Slug = slug;
        ModifiedAt = DateTime.UtcNow;
    }

    public void IncrementUsageCount()
    {
        UsageCount++;
        ModifiedAt = DateTime.UtcNow;
    }

    public void DecrementUsageCount()
    {
        if (UsageCount > 0)
        {
            UsageCount--;
            ModifiedAt = DateTime.UtcNow;
        }
    }
}