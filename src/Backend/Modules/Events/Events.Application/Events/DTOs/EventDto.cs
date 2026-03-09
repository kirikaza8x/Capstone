namespace Events.Application.Events.DTOs;

public sealed record EventCategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

public sealed record EventHashtagDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

public sealed record EventActorImageDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Major { get; init; }
    public string? Image { get; init; }
}

public sealed record EventImageDto
{
    public Guid Id { get; init; }
    public string? ImageUrl { get; init; }
}

public sealed record EventSessionDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
}
