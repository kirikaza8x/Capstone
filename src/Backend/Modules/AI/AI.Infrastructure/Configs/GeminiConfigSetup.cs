
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Infrastructure.Configs
{
    public class GeminiConfigSetup : IConfigureOptions<GeminiConfig>
    {
        private readonly string ConfigurationSectionName = "Gemini";
        private readonly IConfiguration _configuration;

        public GeminiConfigSetup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(GeminiConfig options)
        {
            _configuration.GetSection(ConfigurationSectionName).Bind(options);
        }
    }
}