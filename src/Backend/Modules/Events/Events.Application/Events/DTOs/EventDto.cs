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

public sealed record TicketTypeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Quantity { get; init; }
    public int SoldQuantity { get; set; }
    public int RemainingQuantity { get; set; }
    public Guid? AreaId { get; init; }
    public string? AreaName { get; init; }
    public string? AreaType { get; init; }
}

public sealed record EventSessionDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
}
