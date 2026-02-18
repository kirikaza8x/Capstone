using AutoMapper;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Queries.GetEvent;

internal sealed class GetEventQueryHandler(
    IEventRepository eventRepository,
    IMapper mapper) : IQueryHandler<GetEventQuery, GetEventResponse>
{
    public async Task<Result<GetEventResponse>> Handle(
        GetEventQuery query,
        CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdWithAllDetailsAsync(query.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure<GetEventResponse>(EventErrors.Event.NotFound(query.EventId));

        var response = mapper.Map<GetEventResponse>(@event);

        return Result.Success(response);
    }
}