using AutoMapper;
using Events.Application.Events.Queries.GetEventByUrlPath;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Queries.GetEventByUrlPath;

internal sealed class GetEventByUrlPathQueryHandler(
    IEventRepository eventRepository,
    IMapper mapper) : IQueryHandler<GetEventByUrlPathQuery, GetEventByUrlPathResponse>
{
    public async Task<Result<GetEventByUrlPathResponse>> Handle(
        GetEventByUrlPathQuery query,
        CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByUrlPathAsync(
            query.UrlPath,
            cancellationToken);

        if (@event is null)
            return Result.Failure<GetEventByUrlPathResponse>(
                EventErrors.Event.NotFoundByUrlPath(query.UrlPath));

        var response = mapper.Map<GetEventByUrlPathResponse>(@event);
        return Result.Success(response);
    }
}
