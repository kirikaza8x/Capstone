using Shared.Domain.DDD;

namespace Events.Domain.Entities;

public sealed class Category : Entity<int>
{
    private Category() { }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public static Category Create(string code, string name, string? description)
    {
        return new Category
        {
            Code = code,
            Name = name,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        ModifiedAt = DateTime.UtcNow;
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