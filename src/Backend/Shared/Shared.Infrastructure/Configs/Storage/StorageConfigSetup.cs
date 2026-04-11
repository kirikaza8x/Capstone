using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Shared.Infrastructure.Configs.Storage;

public class StorageConfigSetup(IConfiguration configuration)
    : IConfigureOptions<StorageConfig>
{
    public const string ConfigurationSectionName = "Storage";

    public void Configure(StorageConfig options)
    {
        configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}
