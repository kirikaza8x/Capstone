using Events.Application.Events.DTOs;
using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Queries.GetTicketTypes;

public sealed record GetTicketTypesQuery(
    Guid EventId,
    Guid EventSessionId) : IQuery<IReadOnlyList<TicketTypeDto>>;
