using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Shared.Infrastructure.Configs.Security
{
    public class JwtConfigSetup : IConfigureOptions<JwtConfigs>
    {
        private readonly IConfiguration _configuration;

        public JwtConfigSetup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(JwtConfigs options)
        {
            _configuration.GetSection("JwtConfigs").Bind(options);
        }
    }
}
