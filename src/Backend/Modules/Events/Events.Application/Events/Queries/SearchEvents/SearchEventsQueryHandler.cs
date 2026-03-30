using AutoMapper;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;

namespace Events.Application.Events.Queries.SearchEvents;

internal sealed class SearchEventsQueryHandler(
    IEventRepository eventRepository,
    IMapper mapper) : IQueryHandler<SearchEventsQuery, PagedResult<EventSearchResponse>>
{
    public async Task<Result<PagedResult<EventSearchResponse>>> Handle(
        SearchEventsQuery request,
        CancellationToken cancellationToken)
    {
        var pagedEvents = await eventRepository.SearchEventsAsync(
            request.Keyword,
            request,
            cancellationToken);

        var responseItems = mapper.Map<IReadOnlyList<EventSearchResponse>>(pagedEvents.Items);

        var result = PagedResult<EventSearchResponse>.Create(
            responseItems,
            pagedEvents.PageNumber,
            pagedEvents.PageSize,
            pagedEvents.TotalCount);

        return Result.Success(result);
    }
}
