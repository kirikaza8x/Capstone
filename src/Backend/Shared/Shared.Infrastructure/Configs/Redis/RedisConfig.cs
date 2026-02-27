
namespace Shared.Infrastructure.Configs.Redis;

public class RedisConfig : ConfigBase
{
    public override string SectionName => "Redis";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
    public string? Password { get; set; }
    public string InstanceName { get; set; } = "AIPromo_";

    public string ConnectionString => string.IsNullOrWhiteSpace(Password)
        ? $"{Host}:{Port},abortConnect=false"
        : $"{Host}:{Port},password={Password},abortConnect=false";
}
 