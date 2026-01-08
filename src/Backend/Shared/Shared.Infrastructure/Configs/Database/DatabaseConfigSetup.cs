using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Shared.Infrastructure.Configs.Database;

public class DatabaseConfigSetup : IConfigureOptions<DatabaseConfig>
{
    private readonly string ConfigurationSectionName = "Database";
    private readonly IConfiguration _configuration;

    public DatabaseConfigSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(DatabaseConfig options)
    {
        _configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}