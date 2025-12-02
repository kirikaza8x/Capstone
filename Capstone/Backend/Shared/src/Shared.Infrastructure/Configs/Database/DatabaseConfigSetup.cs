using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace Shared.Infrastructure.Configs.Database
{
    public class DatabaseConfigSetup : IConfigureOptions<DatabaseConfig>
    {
        private readonly string ConfigurationSectionName = "DatabaseConfigurations";
        private readonly IConfiguration _configuration;

        public DatabaseConfigSetup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(DatabaseConfig options)
        {
            // options.ConnectionString = _configuration.GetConnectionString("Database");
            _configuration.GetSection(ConfigurationSectionName).Bind(options);
        }
    }
}