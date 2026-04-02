using Microsoft.AspNetCore.SignalR;
using Ticketing.Application.Abstractions.Notifications;
using Ticketing.Application.Reports.GetCheckInStats;

namespace Ticketing.Infrastructure.SignalR;

internal sealed class SignalRCheckInStatsNotifier(
    IHubContext<TicketHub> hubContext)
    : ICheckInStatsNotifier
{
    public async Task NotifyStatsUpdatedAsync(
        Guid eventId,
        CheckInStatsResponse stats,
        CancellationToken cancellationToken = default)
    {
        await hubContext.Clients
            .Group(SignalRGroupNames.Event(eventId))
            .SendAsync("OnCheckInStatsUpdated", stats, cancellationToken);
    }
}
