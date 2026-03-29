using Events.Application.Events.DTOs;
using Shared.Application.Abstractions.Messaging;

namespace Events.Application.EventMembers.Queries.GetAssignedEvents;

public sealed record GetAssignedEventsQuery : IQuery<IReadOnlyCollection<AssignedEventResponse>>;

public sealed record AssignedEventResponse
{
    public Guid EventId { get; init; }
    public string Title { get; init; }
    public string BannerUrl { get; init; }
    public DateTime? EventStartAt { get; init; }
    public DateTime? EventEndAt { get; init; }
    public IReadOnlyCollection<EventSessionDto> Sessions { get; init; } = [];
};

