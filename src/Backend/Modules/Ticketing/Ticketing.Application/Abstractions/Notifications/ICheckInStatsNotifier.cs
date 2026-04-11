using Ticketing.Application.Reports.GetCheckInStats;

namespace Ticketing.Application.Abstractions.Notifications;

public interface ICheckInStatsNotifier
{
    // using to notify clients about updated check-in stats
    Task NotifyStatsUpdatedAsync(Guid eventId, CheckInStatsResponse stats, CancellationToken cancellationToken = default);
}
