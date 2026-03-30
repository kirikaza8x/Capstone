using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.SignalR;

public class SignalRLogger : ILogger
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _categoryName;

    public SignalRLogger(IServiceScopeFactory scopeFactory, string categoryName)
    {
        _scopeFactory = scopeFactory;
        _categoryName = categoryName;
    }

    IDisposable? ILogger.BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);

        Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var notifier = scope.ServiceProvider.GetService<ILogNotifier>();
                if (notifier != null)
                {
                    await notifier.NotifyAsync($"[{_categoryName}] {message}", logLevel.ToString());
                }
            }
            catch { /* Keep the logger silent on internal failure */ }
        });
    }
}