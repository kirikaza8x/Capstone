using Microsoft.AspNetCore.SignalR;

namespace Ticketing.Infrastructure.SignalR;

public class TicketHub : Hub
{
    public async Task JoinEventGroup(Guid eventId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            SignalRGroupNames.Event(eventId));
    }

    public async Task LeaveEventGroup(Guid eventId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            SignalRGroupNames.Event(eventId));
    }
}

internal static class SignalRGroupNames
{
    public static string Event(Guid eventId) => $"event:{eventId:N}";
}
