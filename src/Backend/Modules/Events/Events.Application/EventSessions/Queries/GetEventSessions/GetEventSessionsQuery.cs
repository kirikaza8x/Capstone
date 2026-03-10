using Events.Application.Events.DTOs;
using Shared.Application.Abstractions.Messaging;

namespace Events.Application.EventSessions.Queries.GetEventSessions;

public sealed record GetEventSessionsQuery(Guid EventId) : IQuery<IReadOnlyList<EventSessionDto>>;