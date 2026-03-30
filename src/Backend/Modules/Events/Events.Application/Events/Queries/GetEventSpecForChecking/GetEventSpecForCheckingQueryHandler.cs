using Events.Domain.Errors;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Queries.GetEventSpecForChecking;

internal class GetEventSpecForCheckingQueryHandler(
    IEventRepository eventRepository) : IQueryHandler<GetEventSpecForCheckingQuery, GetEventSpecForCheckingResponse>
{
    public async Task<Result<GetEventSpecForCheckingResponse>> Handle(GetEventSpecForCheckingQuery query, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(query.EventId, cancellationToken);
        if (@event is null)
            return Result.Failure<GetEventSpecForCheckingResponse>(EventErrors.Event.NotFound(query.EventId));

        return Result.Success(new GetEventSpecForCheckingResponse
        (
            @event.Spec,
            @event.SpecImage
        ));
    }
}
