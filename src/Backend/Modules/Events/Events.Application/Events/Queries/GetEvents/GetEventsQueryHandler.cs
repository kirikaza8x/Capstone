using AutoMapper;
using Events.Domain.Enums;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Caching;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;

namespace Events.Application.Events.Queries.GetEvents;

internal sealed class GetEventsQueryHandler(
    IEventRepository eventRepository,
    IMapper mapper,
    ICacheService cacheService) : IQueryHandler<GetEventsQuery, PagedResult<EventResponse>>
{
    public async Task<Result<PagedResult<EventResponse>>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = CacheKeys.Events.GetList(
            request.PageNumber ?? 1,
            request.PageSize ?? 10,
            request.SortColumn,
            request.SortOrder
        );

        var pagedResult = await cacheService.GetOrCreateAsync(
            key: cacheKey,
            factory: async (token) =>
            {
                var pagedEvents = await eventRepository.GetAllWithPagingAsync(
                    pagedQuery: request,
                    predicate: e => e.Status == EventStatus.Published,
                    cancellationToken: token
                );

                var responseItems = mapper.Map<IReadOnlyList<EventResponse>>(pagedEvents.Items);

                return PagedResult<EventResponse>.Create(
                    responseItems,
                    pagedEvents.PageNumber,
                    pagedEvents.PageSize,
                    pagedEvents.TotalCount
                );
            },
            expiration: TimeSpan.FromMinutes(5),
            cancellationToken: cancellationToken
        );

        return pagedResult!;
    }
}
