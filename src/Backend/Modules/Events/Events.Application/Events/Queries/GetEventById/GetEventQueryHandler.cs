using AutoMapper;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Caching;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Queries.GetEventById;

internal sealed class GetEventQueryHandler(
    IEventRepository eventRepository,
    IMapper mapper,
    ICacheService cacheService
    ) : IQueryHandler<GetEventQuery, GetEventResponse>
{
    public async Task<Result<GetEventResponse>> Handle(
        GetEventQuery query,
        CancellationToken cancellationToken)
    {
        string cacheKey = CacheKeys.Events.GetById(query.EventId);

        var @event = await eventRepository.GetByIdWithAllDetailsAsync(query.EventId, cancellationToken);

        var eventResponse = await cacheService.GetOrCreateAsync(
            key: cacheKey,
            factory: async (token) =>
            {
                var @event = await eventRepository.GetByIdWithAllDetailsAsync(query.EventId, token);

                if (@event is null)
                    return null;

                return mapper.Map<GetEventResponse>(@event);
            },
            TimeSpan.FromHours(1),
            cancellationToken);

        if (eventResponse is null)
        {
            return Result.Failure<GetEventResponse>(EventErrors.Event.NotFound(query.EventId));
        }

        return Result.Success(eventResponse);
    }
}