using AutoMapper;
using Events.Application.Events.Extensions;
using Events.Domain.Enums;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Events.Application.Events.Queries.GetEventsForStaff;

internal sealed class GetEventsForStaffQueryHandler(
    IEventRepository eventRepository,
    IMapper mapper)
    : IQueryHandler<GetEventsForStaffQuery, PagedResult<EventsForStaffResponse>>
{
    public async Task<Result<PagedResult<EventsForStaffResponse>>> Handle(
        GetEventsForStaffQuery query,
        CancellationToken cancellationToken)
    {
        var statuses = query.Statuses.ParseStatuses();

        if (statuses.Count == 0)
        {
            statuses =
            [
                EventStatus.PendingReview,
                EventStatus.PendingCancellation
            ];
        }

        var pagedQuery = string.IsNullOrWhiteSpace(query.SortColumn)
            ? query with { SortColumn = "CreatedAt", SortOrder = SortOrder.Descending }
            : query;

        var pagedEvents = await eventRepository.GetAllWithPagingAsync(
            pagedQuery,
            e => statuses.Contains(e.Status) &&
                 (string.IsNullOrWhiteSpace(query.Title) || e.Title.Contains(query.Title)),
            includes: [],
            cancellationToken: cancellationToken);

        var responseItems = mapper.Map<IReadOnlyList<EventsForStaffResponse>>(pagedEvents.Items);

        var result = PagedResult<EventsForStaffResponse>.Create(
            responseItems,
            pagedEvents.PageNumber,
            pagedEvents.PageSize,
            pagedEvents.TotalCount);

        return Result.Success(result);
    }
}
