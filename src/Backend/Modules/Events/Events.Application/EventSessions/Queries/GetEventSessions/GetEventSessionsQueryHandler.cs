using AutoMapper;
using Events.Application.Events.DTOs;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.EventSessions.Queries.GetEventSessions;

internal sealed class GetEventSessionsQueryHandler(
    IEventRepository eventRepository,
    IMapper mapper) : IQueryHandler<GetEventSessionsQuery, IReadOnlyList<EventSessionDto>>
{
    public async Task<Result<IReadOnlyList<EventSessionDto>>> Handle(
        GetEventSessionsQuery query,
        CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdWithSessionsAsync(query.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure<IReadOnlyList<EventSessionDto>>(EventErrors.Event.NotFound(query.EventId));

        var response = mapper.Map<IReadOnlyList<EventSessionDto>>(
            @event.Sessions.OrderBy(s => s.StartTime));

        return Result.Success(response);
    }
}
