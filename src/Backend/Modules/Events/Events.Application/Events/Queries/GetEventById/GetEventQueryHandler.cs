using AutoMapper;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Caching;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Queries.GetEventById;

internal sealed class GetEventByUrlPathQueryHandler(
    IEventRepository eventRepository,
    IMapper mapper
    ) : IQueryHandler<GetEventQuery, GetEventResponse>
{
    public async Task<Result<GetEventResponse>> Handle(
        GetEventQuery query,
        CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetDetailsByIdAsync(query.EventId, cancellationToken);

        if (@event is null)
        {
            return Result.Failure<GetEventResponse>(EventErrors.Event.NotFound(query.EventId));
        }

        var eventResponse = mapper.Map<GetEventResponse>(@event);

        return Result.Success(eventResponse);
    }
}
