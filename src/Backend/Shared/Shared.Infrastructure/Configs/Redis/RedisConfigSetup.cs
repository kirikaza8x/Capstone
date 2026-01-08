using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Shared.Infrastructure.Configs.Redis;

public class RedisConfigSetup : IConfigureOptions<RedisConfig>
{
    private const string ConfigurationSectionName = "Redis";
    private readonly IConfiguration _configuration;

    public RedisConfigSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(RedisConfig options)
    {
        _configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}