using Shared.Domain.DDD;

namespace Users.Domain.Entities;

public class Policy : Entity<Guid>
{
    public string Type { get; private set; } = string.Empty;
    public string? FileUrl { get; private set; }
    public string Description { get; private set; } = string.Empty;

    private Policy() { }

    public static Policy Create(string type, string? fileUrl, string description)
    {
        return new Policy
        {
            Id = Guid.NewGuid(),
            Type = type.Trim(),
            FileUrl = string.IsNullOrWhiteSpace(fileUrl) ? null : fileUrl.Trim(),
            Description = description.Trim()
        };
    }

    public void Update(string type, string? fileUrl, string description)
    {
        Type = type.Trim();
        FileUrl = string.IsNullOrWhiteSpace(fileUrl) ? null : fileUrl.Trim();
        Description = description.Trim();
    }
}
