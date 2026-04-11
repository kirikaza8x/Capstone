namespace Shared.Application.Abstractions.SignalR;
public interface ILogNotifier
{
    Task NotifyAsync(string message, string level);
}
