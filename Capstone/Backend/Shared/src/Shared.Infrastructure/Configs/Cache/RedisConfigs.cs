namespace Shared.Infrastructure.Configs.Cache;

public class RedisConfigs
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
    public string? Password { get; set; }
    public string InstanceName { get; set; } = "AppConfigs_";

    // Helper to build the connection string for StackExchange.Redis
    public string ConnectionString => Password != null 
        ? $"{Host}:{Port},password={Password}" 
        : $"{Host}:{Port}";
}