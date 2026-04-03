using AI.Domain.Enums;
using Shared.Domain.DDD;

namespace AI.Domain.Entities;

public class AiPackage : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public AiPackageType Type { get; private set; }
    public decimal Price { get; private set; }
    public int TokenQuota { get; private set; }
    public bool IsActive { get; private set; }

    private AiPackage() { }

    public static AiPackage Create(
        string name,
        string? description,
        AiPackageType type,
        decimal price,
        int tokenQuota)
    {
        return new AiPackage
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Type = type,
            Price = price,
            TokenQuota = tokenQuota,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void Update(
        string name,
        string? description,
        AiPackageType type,
        decimal price,
        int tokenQuota,
        bool isActive)
    {
        Name = name.Trim();
        Description = description?.Trim();
        Type = type;
        Price = price;
        TokenQuota = tokenQuota;
        IsActive = isActive;
    }
}
