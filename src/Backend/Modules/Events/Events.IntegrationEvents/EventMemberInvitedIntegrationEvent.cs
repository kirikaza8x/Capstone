using Shared.Application.Abstractions.EventBus;

namespace Events.IntegrationEvents;

public sealed record EventMemberInvitedIntegrationEvent : IntegrationEvent
{
    public Guid EventId { get; }
    public string EventTitle { get; }
    public Guid EventMemberId { get; }
    public Guid UserId { get; }
    public string Email { get; }

    public EventMemberInvitedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid eventId,
        string eventTitle,
        Guid eventMemberId,
        Guid userId,
        string email)
        : base(id, occurredOnUtc)
    {
        EventId = eventId;
        EventTitle = eventTitle;
        EventMemberId = eventMemberId;
        UserId = userId;
        Email = email;
    }
}
