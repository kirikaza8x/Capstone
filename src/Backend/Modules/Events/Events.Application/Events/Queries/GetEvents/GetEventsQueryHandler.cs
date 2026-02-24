using AutoMapper;
using Events.Domain.Enums;
using Events.Domain.Repositories;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;

namespace Events.Application.Events.Queries.GetEvents;

internal sealed class GetEventsQueryHandler(
    IEventRepository eventRepository,
    IMapper mapper) : IQueryHandler<GetEventsQuery, PagedResult<EventResponse>>
{
    public async Task<Result<PagedResult<EventResponse>>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var pagedEvents = await eventRepository.GetAllWithPagingAsync(
            pagedQuery: request,
            predicate: e => e.Status == EventStatus.Published, 
            cancellationToken: cancellationToken
        );

        var responseItems = mapper.Map<IReadOnlyList<EventResponse>>(pagedEvents.Items);

        var pagedResult = PagedResult<EventResponse>.Create(
            responseItems,
            pagedEvents.PageNumber,
            pagedEvents.PageSize,
            pagedEvents.TotalCount
        );

        return pagedResult;
    }
}
