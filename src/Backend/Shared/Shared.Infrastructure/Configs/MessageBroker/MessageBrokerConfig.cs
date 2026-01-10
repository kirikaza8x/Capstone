namespace Shared.Infrastructure.Configs.MessageBroker;

public sealed class MessageBrokerConfig
{
    public string Host { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public ushort Heartbeat { get; set; } = 10;

    public int RequestedConnectionTimeout { get; set; } = 30000;
}
