// Shared/Infrastructure/Service/SignalRLogNotifier.cs
using Microsoft.AspNetCore.SignalR;
using Shared.Application.Abstractions.SignalR;

public class SignalRLogNotifier : ILogNotifier
{
    private readonly IHubContext<LogHub> _hubContext;

    public SignalRLogNotifier(IHubContext<LogHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyAsync(string message, string level)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveLog", new 
        { 
            message, 
            level,
            timestamp = DateTime.Now.ToString("HH:mm:ss") 
        });
    }
}