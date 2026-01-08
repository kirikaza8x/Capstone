
namespace Shared.Infrastructure.Configs.Redis;

public class RedisConfig
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
    public string? Password { get; set; }
    public string InstanceName { get; set; } = "AppConfigs_";

    public string ConnectionString => Password != null
        ? $"{Host}:{Port},password={Password}"
        : $"{Host}:{Port}";
}
 