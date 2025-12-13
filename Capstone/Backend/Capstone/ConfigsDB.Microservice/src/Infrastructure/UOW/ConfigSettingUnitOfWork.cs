using ConfigsDB.Domain.Repositories;
using ConfigsDB.Domain.UnitOfWork;
using ConfigsDB.Infrastructure.Persistence.Contexts;
using Shared.Infrastructure.Common;


namespace ConfigsDB.Infrastructure.UnitOfWork
{
    public class ConfigSettingUnitOfWork 
        : GenericUnitOfWork<ConfigSettingDbContext>, IConfigSettingUnitOfWork
    {
        private readonly IConfigSettingRepository _configSettingRepository;

        public ConfigSettingUnitOfWork(ConfigSettingDbContext dbContext, IConfigSettingRepository configSettingRepository)
            : base(dbContext)
        {
            _configSettingRepository = configSettingRepository;
        }

        public IConfigSettingRepository ConfigSettings => _configSettingRepository;
    }
}
