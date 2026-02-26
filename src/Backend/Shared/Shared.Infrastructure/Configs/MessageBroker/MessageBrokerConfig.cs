namespace Shared.Infrastructure.Configs.MessageBroker;

public sealed class MessageBrokerConfig : ConfigBase
{
    public override string SectionName => "MessageBroker";
    public string Host { get; set; } = string.Empty; 
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public ushort Heartbeat { get; set; } = 10;
    public int RequestedConnectionTimeout { get; set; } = 30000;
}
