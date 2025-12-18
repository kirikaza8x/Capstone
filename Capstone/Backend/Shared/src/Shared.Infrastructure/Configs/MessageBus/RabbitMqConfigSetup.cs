using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Shared.Infrastructure.Configs.MessageBus
{
    public class RabbitMqConfigSetup : IConfigureOptions<RabbitMqConfigs>
    {
        private const string ConfigurationSectionName = "RabbitMQ";
        private readonly IConfiguration _configuration;

        public RabbitMqConfigSetup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(RabbitMqConfigs options)
        {
            _configuration.GetSection(ConfigurationSectionName).Bind(options);
        }
    }
}