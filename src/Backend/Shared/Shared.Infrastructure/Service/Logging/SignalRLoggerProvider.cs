// Shared/Infrastructure/Logging/SignalRLoggerProvider.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class SignalRLoggerProvider : ILoggerProvider
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SignalRLoggerProvider(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public ILogger CreateLogger(string categoryName) => new SignalRLogger(_scopeFactory, categoryName);
    public void Dispose() { }
}