using Shared.Application.Messaging;

namespace Events.Application.Events.Queries.GetEventById;

public sealed record GetEventQuery(Guid EventId) : IQuery<GetEventResponse>;

public sealed record GetEventResponse
{
    public Guid Id { get; init; }
    public Guid OrganizerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? BannerUrl { get; init; }
    public string Location { get; init; } = string.Empty;
    public string? MapUrl { get; init; }
    public string Description { get; init; } = string.Empty;
    public string UrlPath { get; init; } = string.Empty;
    public int EventCategoryId { get; init; }
    public DateTime? TicketSaleStartAt { get; init; }
    public DateTime? TicketSaleEndAt { get; init; }
    public DateTime? EventStartAt { get; init; }
    public DateTime? EventEndAt { get; init; }
    public string Policy { get; init; } = string.Empty;
    public string? Spec { get; init; }
    public string? SeatmapImage { get; init; }
    public bool IsEmailReminderEnabled { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }

    public IReadOnlyList<EventImageDto> Images { get; init; } = [];
    public IReadOnlyList<EventSessionDto> Sessions { get; init; } = [];
    public IReadOnlyList<EventHashtagDto> Hashtags { get; init; } = [];
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

public sealed record EventHashtagDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}