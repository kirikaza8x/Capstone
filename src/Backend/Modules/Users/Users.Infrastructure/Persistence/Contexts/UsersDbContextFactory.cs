using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.Configs.Database;

namespace Users.Infrastructure.Persistence.Contexts
{
    public class UsersDbContextFactory : IDesignTimeDbContextFactory<UserModuleDbContext>
    {
        public UserModuleDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var dbConfig = new DatabaseConfig();
            configuration.GetSection("Database").Bind(dbConfig);

            var optionsBuilder = new DbContextOptionsBuilder<UserModuleDbContext>();
            optionsBuilder.UseNpgsql(dbConfig.ConnectionString);

            return new UserModuleDbContext(optionsBuilder.Options);
        }
    }
}
