using Shared.Application.Messaging;

namespace Events.Application.Events.Queries.GetEvent;

public sealed record GetEventQuery(Guid EventId) : IQuery<GetEventResponse>;

public sealed record GetEventResponse(
    Guid Id,
    Guid OrganizerId,
    string Title,
    string Status,
    string? BannerUrl,
    string Location,
    string? MapUrl,
    string Description,
    string UrlPath,
    int EventCategoryId,
    DateTime? TicketSaleStartAt,
    DateTime? TicketSaleEndAt,
    DateTime? EventStartAt,
    DateTime? EventEndAt,
    string Policy,
    string? Spec,
    string? SeatmapImage,
    bool IsEmailReminderEnabled,
    DateTime CreatedAt,
    DateTime? ModifiedAt,
    IReadOnlyList<EventImageDto> Images,
    IReadOnlyList<EventSessionDto> Sessions,
    IReadOnlyList<EventHashtagDto> Hashtags);

public sealed record EventImageDto(Guid Id, string? ImageUrl);

public sealed record EventSessionDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime EndTime);

public sealed record EventHashtagDto(Guid Id, string Name);