using ConfigsDB.Domain.Entities;
using ConfigsDB.Domain.Repositories;
using ConfigsDB.Infrastructure.Persistence.Contexts;
using Shared.Infrastructure.Common;

namespace ConfigsDB.Infrastructure.Repositories
{
    public class ConfigSettingBulkRepository 
        : BulkRepository<ConfigSetting>, IConfigSettingBulkRepository
    {
        public ConfigSettingBulkRepository(ConfigSettingDbContext dbContext) 
            : base(dbContext) { }

        
    }
}
