using Events.Domain.Errors;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Queries.GetEventSpec;

internal sealed class GetEventSpecQueryHandler(
    IEventRepository eventRepository) : IQueryHandler<GetEventSpecQuery, GetEventSpecResponse>
{
    public async Task<Result<GetEventSpecResponse>> Handle(
        GetEventSpecQuery query,
        CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(query.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure<GetEventSpecResponse>(EventErrors.Event.NotFound(query.EventId));

        return Result.Success(new GetEventSpecResponse(@event.Id, @event.Spec));
    }
}
