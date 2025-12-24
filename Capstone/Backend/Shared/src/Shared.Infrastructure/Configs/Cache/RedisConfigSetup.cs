using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Shared.Infrastructure.Configs.Cache;

public class RedisConfigSetup : IConfigureOptions<RedisConfigs>
{
    private const string ConfigurationSectionName = "Redis";
    private readonly IConfiguration _configuration;

    public RedisConfigSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(RedisConfigs options)
    {
        _configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}