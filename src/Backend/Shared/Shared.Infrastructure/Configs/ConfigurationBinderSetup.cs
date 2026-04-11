using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Shared.Infrastructure.Configs
{
    public class ConfigurationBinderSetup<T> : IConfigureOptions<T> where T : ConfigBase
    {
        private readonly IConfiguration _configuration;

        public ConfigurationBinderSetup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(T options)
        {
            _configuration.GetSection(options.SectionName).Bind(options);
        }
    }
}
