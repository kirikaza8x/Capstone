using AutoMapper;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Caching;
using Shared.Application.Abstractions.Messaging;
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
        var pagedEvents = await eventRepository.GetPublishedWithCategoriesAsync(request, cancellationToken);

        var responseItems = mapper.Map<IReadOnlyList<EventResponse>>(pagedEvents.Items);

        var result = PagedResult<EventResponse>.Create(
            responseItems,
            pagedEvents.PageNumber,
            pagedEvents.PageSize,
            pagedEvents.TotalCount);

        return Result.Success(result);
    }
}
