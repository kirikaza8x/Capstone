using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.Configs.Database;

namespace Events.Infrastructure.Data
{
    public class EventsDbContextFactory : IDesignTimeDbContextFactory<EventsDbContext>
    {
        public EventsDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var dbConfig = new DatabaseConfig();
            configuration.GetSection("Database").Bind(dbConfig);

            var optionsBuilder = new DbContextOptionsBuilder<EventsDbContext>();
            optionsBuilder.UseNpgsql(dbConfig.ConnectionString);

            return new EventsDbContext(optionsBuilder.Options);
        }
    }
}
