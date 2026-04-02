using MediatR;
using Ticketing.Application.Abstractions.Notifications;
using Ticketing.Application.Reports.GetCheckInStats;

namespace Ticketing.Application.Services;

public sealed class CheckInStatsBroadcaster(
    ISender sender,
    ICheckInStatsNotifier notifier) : ICheckInStatsBroadcaster
{
    public async Task BroadcastAsync(
        Guid eventId,
        Guid eventSessionId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetCheckInStatsQuery(eventId, eventSessionId),
            cancellationToken);

        if (result.IsSuccess)
        {
            await notifier.NotifyStatsUpdatedAsync(eventId, result.Value, cancellationToken);
        }
    }
}
