using Events.Domain.Enums;
using Shared.Application.Messaging;

namespace Events.Application.Events.Queries.GetEvents;

public sealed record GetEventsQuery(
    string? SearchTerm = null,
    EventStatus? Status = null,
    int? CategoryId = null,
    Guid? OrganizerId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int PageNumber = 1,
    int PageSize = 10,
    string? SortBy = null,
    bool IsDescending = true) : IQuery<GetEventsResponse>;

public sealed record GetEventsResponse(
    IReadOnlyList<EventSummaryDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

public sealed record EventSummaryDto(
    Guid Id,
    string Title,
    string Status,
    string? BannerUrl,
    string Location,
    DateTime? EventStartAt,
    DateTime? EventEndAt,
    string UrlPath,
    int EventCategoryId,
    DateTime CreatedAt);