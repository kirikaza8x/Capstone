using AutoMapper;
using Events.Application.Events.Queries.GetEvents;
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
    IMapper mapper) : IQueryHandler<GetEventsByOrganizerQuery, PagedResult<EventResponse>>
{
    public async Task<Result<PagedResult<EventResponse>>> Handle(
        GetEventsByOrganizerQuery query,
        CancellationToken cancellationToken)
    {
        var organizerId = currentUserService.UserId;
        var pagedQuery = string.IsNullOrWhiteSpace(query.SortColumn)
            ? query with { SortColumn = "CreatedAt", SortOrder = SortOrder.Descending }
            : query;

        var pagedEvents = await eventRepository.GetAllWithPagingAsync(
            pagedQuery: pagedQuery,
            predicate: e => e.OrganizerId == organizerId
                         && (query.Status == null || e.Status == query.Status),
            includes: [e => e.EventCategories],
            cancellationToken: cancellationToken);

        var responseItems = mapper.Map<IReadOnlyList<EventResponse>>(pagedEvents.Items);

        var result = PagedResult<EventResponse>.Create(
            responseItems,
            pagedEvents.PageNumber,
            pagedEvents.PageSize,
            pagedEvents.TotalCount);

        return Result.Success(result);
    }
}