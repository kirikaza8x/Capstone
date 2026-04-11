using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Events.Application.Events.Queries.GetEventsByOrganizer;

public record GetEventsByOrganizerQuery : PagedQuery, IQuery<PagedResult<EventsByOrganizerResponse>>
{
    public string? Statuses { get; init; }
    public string? Title { get; init; }
}

public sealed record EventsByOrganizerResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? BannerUrl { get; init; }
    public string Location { get; init; } = string.Empty;
    public DateTime? EventStartAt { get; init; }
    public DateTime? EventEndAt { get; init; }
    public string UrlPath { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string? CancellationReason { get; private set; }
    public string? PublishRejectionReason { get; private set; }
    public string? CancellationRejectionReason { get; private set; }
    public string? SuspensionReason { get; private set; }
    public DateTime? SuspendedAt { get; private set; }
    public DateTime? SuspendedUntilAt { get; private set; }
    public Guid? SuspendedBy { get; private set; }
}
