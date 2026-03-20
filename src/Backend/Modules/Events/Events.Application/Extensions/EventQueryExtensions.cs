using Events.Domain.Enums;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Events.Application.Events.Extensions;

internal static class EventQueryExtensions
{
    public static List<EventStatus> ParseStatuses(this string? statuses)
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
}
