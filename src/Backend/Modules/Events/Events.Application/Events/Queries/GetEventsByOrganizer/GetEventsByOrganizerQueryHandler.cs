using AutoMapper;
using Events.Domain.Enums;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Events.Application.Events.Queries.GetEventsByOrganizer;

internal sealed class GetEventsByOrganizerQueryHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService,
    IMapper mapper)
    : IQueryHandler<GetEventsByOrganizerQuery, PagedResult<EventsByOrganizerResponse>>
{
    public async Task<Result<PagedResult<EventsByOrganizerResponse>>> Handle(
        GetEventsByOrganizerQuery query,
        CancellationToken cancellationToken)
    {
        var organizerId = currentUserService.UserId;

        var statuses = ParseStatuses(query.Statuses);

        var pagedQuery = EnsureDefaultSorting(query);

        var pagedEvents = await eventRepository.GetAllWithPagingAsync(
            pagedQuery,
            e =>
                e.OrganizerId == organizerId &&
                (statuses.Count == 0 || statuses.Contains(e.Status)) &&
                (string.IsNullOrWhiteSpace(query.Title) ||
                 e.Title.Contains(query.Title)),
            includes: [e => e.EventCategories],
            cancellationToken: cancellationToken);

        var responseItems = mapper.Map<IReadOnlyList<EventsByOrganizerResponse>>(pagedEvents.Items);

        var result = PagedResult<EventsByOrganizerResponse>.Create(
            responseItems,
            pagedEvents.PageNumber,
            pagedEvents.PageSize,
            pagedEvents.TotalCount);

        return Result.Success(result);
    }

    private static List<EventStatus> ParseStatuses(string? statuses)
    {
        if (string.IsNullOrWhiteSpace(statuses))
            return [];

        return statuses
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => Enum.TryParse<EventStatus>(s, true, out var parsed) ? parsed : (EventStatus?)null)
            .Where(s => s.HasValue)
            .Select(s => s!.Value)
            .ToList();
    }

    private static GetEventsByOrganizerQuery EnsureDefaultSorting(GetEventsByOrganizerQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.SortColumn))
            return query;

        return query with
        {
            SortColumn = "CreatedAt",
            SortOrder = SortOrder.Descending
        };
    }
}