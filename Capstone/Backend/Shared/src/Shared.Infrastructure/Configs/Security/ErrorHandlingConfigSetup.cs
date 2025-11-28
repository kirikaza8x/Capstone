using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Shared.Infrastructure.Configs.Security
{
    /// <summary>
    /// Binds the "ErrorHandling" section of appsettings.json to ErrorHandlingConfigs
    /// </summary>
    public class ErrorHandlingConfigSetup : IConfigureOptions<ErrorHandlingConfigs>
    {
        private readonly IConfiguration _configuration;

        public ErrorHandlingConfigSetup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(ErrorHandlingConfigs options)
        {
            _configuration.GetSection("ErrorHandling").Bind(options);
        }
    }
}
