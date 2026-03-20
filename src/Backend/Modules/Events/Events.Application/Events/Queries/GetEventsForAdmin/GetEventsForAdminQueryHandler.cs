using AutoMapper;
using Events.Application.Events.Extensions;
using Events.Domain.Enums;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Events.Application.Events.Queries.GetEventsForAdmin;

internal sealed class GetEventsForAdminQueryHandler(
    IEventRepository eventRepository,
    IMapper mapper)
    : IQueryHandler<GetEventsForAdminQuery, PagedResult<EventsForAdminResponse>>
{
    public async Task<Result<PagedResult<EventsForAdminResponse>>> Handle(
        GetEventsForAdminQuery query,
        CancellationToken cancellationToken)
    {
        var statuses = query.Statuses.ParseStatuses();
        var pagedQuery = string.IsNullOrWhiteSpace(query.SortColumn)
            ? query with { SortColumn = "CreatedAt", SortOrder = SortOrder.Descending }
            : query;

        var pagedEvents = await eventRepository.GetAllWithPagingAsync(
            pagedQuery,
            e =>
                e.Status != EventStatus.Draft &&
                (!query.OrganizerId.HasValue || e.OrganizerId == query.OrganizerId.Value) &&
                (statuses.Count == 0 || statuses.Contains(e.Status)) &&
                (string.IsNullOrWhiteSpace(query.Title) || e.Title.Contains(query.Title)),
            includes: [],
            cancellationToken: cancellationToken);

        var responseItems = mapper.Map<IEnumerable<EventsForAdminResponse>>(pagedEvents.Items);

        var sortedResponseItems = string.IsNullOrWhiteSpace(query.SortColumn)
            ? responseItems
                .OrderBy(x => x.Status == EventStatus.Published.ToString() ? 0 : 1)
                .ThenByDescending(x => x.CreatedAt)
                .ToList()
            : responseItems.ToList();

        var result = PagedResult<EventsForAdminResponse>.Create(
            sortedResponseItems,
            pagedEvents.PageNumber,
            pagedEvents.PageSize,
            pagedEvents.TotalCount);

        return Result.Success(result);
    }
}
