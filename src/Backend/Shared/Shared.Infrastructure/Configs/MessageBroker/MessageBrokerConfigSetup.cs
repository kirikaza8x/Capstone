using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Shared.Infrastructure.Configs.MessageBroker;

public sealed class MessageBrokerConfigSetup : IConfigureOptions<MessageBrokerConfig>
{
    private const string ConfigurationSectionName = "MessageBroker";
    private readonly IConfiguration _configuration;

    public MessageBrokerConfigSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(MessageBrokerConfig options)
    {
        _configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}